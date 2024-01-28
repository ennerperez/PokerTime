namespace PokerTime.Web.Tests.Integration.Common {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Application.Common.Models;
    using Application.Poker.Commands;
    using Application.PredefinedParticipantColors.Queries.GetAvailablePredefinedParticipantColors;
    using Application.Sessions.Commands.JoinPokerSession;
    using Application.Sessions.Queries.GetParticipantsInfo;
    using Application.SessionWorkflows.Commands;
    using Application.Symbols.Queries;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using PokerTime.Common;

    public sealed class TestCaseBuilder {
        private readonly IServiceScope _scope;
        private readonly string _sessionId;
        private readonly Queue<Func<Task>> _actions;
        private readonly Dictionary<string, ParticipantInfo> _participators;
        private readonly FriendlyIdEntityDictionary _entityIds;

        private (Type Type, int Id) _lastAddedItem;

        public TestCaseBuilder(IServiceScope scope, string sessionId) {
            _scope = scope;
            _sessionId = sessionId;
            _actions = new Queue<Func<Task>>();
            _participators = new Dictionary<string, ParticipantInfo>(StringComparer.InvariantCultureIgnoreCase);
            _entityIds = new FriendlyIdEntityDictionary();
        }

        public TestCaseBuilder HasExistingParticipant(string participantName) {
            _actions.Enqueue(async () => {
                TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] attempting to record presence of existing participant [{participantName}]");

                var dbContext = _scope.ServiceProvider.GetRequiredService<IPokerTimeDbContext>();
                var participant = await dbContext.Participants.FirstAsync(x => x.Name == participantName && x.Session.UrlId.StringId == _sessionId);

                _participators.Add(participant.Name, new ParticipantInfo {
                    Id = participant.Id,
                    Name = participant.Name,
                    Color = new ColorModel(), // Doesn't matter
                    IsFacilitator = participant.IsFacilitator
                });

                RecordAddedId<Participant>(participant.Id);

                TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] recorded presence of existing participant [{participantName}] with ID #{participant.Id}");
            });
            return this;
        }

        public TestCaseBuilder WithParticipant(string name, bool isFacilitator, string passphrase = null) {
            string RandomByte() {
                return TestContext.CurrentContext.Random.NextByte().ToString("X2", Culture.Invariant);
            }

            AvailableParticipantColorModel availableParticipantColor = null;
            _actions.Enqueue(async () => {
                _scope.SetNoAuthenticationInfo();
                var yellowColor = new ColorModel {
                    R = Color.Yellow.R,
                    G = Color.Yellow.G,
                    B = Color.Yellow.B,
                };

                var response = await _scope.Send(new GetAvailablePredefinedParticipantColorsQuery(_sessionId));
                availableParticipantColor = response.FirstOrDefault(x => !x.HasSameColors(yellowColor)); // Yellow is a bad color for testing
            });

            return EnqueueMediatorAction(() => new JoinPokerSessionCommand {
                Name = name,
                Color = availableParticipantColor?.HexString ?? (RandomByte() + RandomByte() + RandomByte()),
                JoiningAsFacilitator = isFacilitator,
                Passphrase = passphrase,
                SessionId = _sessionId
            }, p => {
                if (_participators.ContainsKey(p.Name)) {
                    Assert.Inconclusive($"Trying to register existing participant: {p.Name}");
                }

                RecordAddedId<ParticipantInfo>(p.Id);
                _participators.Add(p.Name, p);
            });
        }

        public TestCaseBuilder NewRound(string title) {
            EnqueueMediatorAction(
                () => new InitiateDiscussionStageCommand { UserStoryTitle = title, SessionId = _sessionId },
                () => Task.CompletedTask);

            return EnqueueMediatorAction(
                () => new InitiateEstimationStageCommand { SessionId = _sessionId },
                () => Task.CompletedTask);
        }

        public TestCaseBuilder CloseEstimationPhase() {
            return EnqueueMediatorAction(
                () => new InitiateEstimationDiscussionStageCommand { SessionId = _sessionId },
                () => Task.CompletedTask);
        }


        public TestCaseBuilder PlayCard(string participantName, string stringValue) {
            ICollection<SymbolModel> symbols = null;
            EnqueueMediatorAction(participantName, () => {
                var dbContext =
                    _scope.ServiceProvider.GetRequiredService<IPokerTimeDbContext>();

                var session = dbContext.Sessions.First(x => x.UrlId.StringId == _sessionId);

                return new GetSymbolsQuery(session.SymbolSetId);
            }, r => symbols = r.Symbols);

            EnqueueMediatorAction(participantName,
                () => {
                    if (symbols == null) {
                        throw new InvalidOperationException("Symbols query didn't return a response");
                    }

                    var requestedSymbol = symbols.FirstOrDefault(x => x.AsString == stringValue);
                    if (requestedSymbol == null) {
                        throw new InvalidOperationException(
                            $"Unable to find symbol '{stringValue}' in list of symbols: {string.Join("|", symbols.Select(x => x.AsString))}");
                    }

                    var dbContext =
                        _scope.ServiceProvider.GetRequiredService<IPokerTimeDbContext>();
                    var userStoryId = dbContext.UserStories.
                        Where(x => x.Session.UrlId.StringId == _sessionId).
                        OrderByDescending(x => x.Id).
                        Select(x => x.Id).
                        FirstOrDefault();

                    return new PlayCardCommand(_sessionId, userStoryId, requestedSymbol.Id);
                },
                () => Task.CompletedTask);

            return this;
        }

        public TestCaseBuilder OutputId(Action<int> callback) {
            _actions.Enqueue(() => {
                if (_lastAddedItem == default) {
                    throw new InvalidOperationException("A call to OutputId should follow a call to an entity creating action");
                }

                TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] Outputting last added item {_lastAddedItem.Type} with ID #{_lastAddedItem.Id} to callback ({callback.GetMethodInfo().Name})");
                callback.Invoke(_lastAddedItem.Id);

                return Task.CompletedTask;
            });

            return this;
        }

        public TestCaseBuilder Callback(Action<TestCaseBuilder> callback) {
            _actions.Enqueue(() => {
                callback(this);
                return Task.CompletedTask;
            });

            return this;
        }

        /// <summary>
        /// Call after entity creation actions
        /// </summary>
        /// <param name="friendlyId"></param>
        /// <returns></returns>
        public TestCaseBuilder WithId(string friendlyId) {
            _actions.Enqueue(() => {
                if (_lastAddedItem == default) {
                    throw new InvalidOperationException("A call to WithId should follow a call to an entity creating action");
                }

                _entityIds.Set(friendlyId, _lastAddedItem.Type, _lastAddedItem.Id);

                return Task.CompletedTask;
            });

            return this;
        }
        public TestCaseBuilder WithSessionStage(SessionStage stage) => EnqueueSessionAction(r => r.CurrentStage = stage);

        private ParticipantInfo GetParticipatorInfo(string name) {
            if (!_participators.TryGetValue(name, out var val)) {
                Assert.Inconclusive($"Test case error: participantName {name} not found");
                return null;
            }

            return val;
        }

        public async Task Build() {
            var actionNumber = 1;
            while (_actions.TryDequeue(out var action)) {
                try {
                    await action();
                }
                catch (Exception ex) {
                    throw new InvalidOperationException($"Error execution action #{actionNumber}: {action}", ex);
                }

                actionNumber++;
            }
        }

        private void RecordAddedId<T>(int id) {
            TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] Recording last added item: [{typeof(T)}] with ID #{id}");
            _lastAddedItem = (typeof(T), id);
        }

        private TestCaseBuilder EnqueueSessionAction(Action<Session> action) {
            _actions.Enqueue(() => _scope.SetSession(_sessionId, action));

            return this;
        }

        private TestCaseBuilder EnqueueMediatorAction(string participantName, Func<IRequest> requestFunc, Func<Task> responseProcessor) {
            _actions.Enqueue(async () => {
                var request = requestFunc();

                if (participantName == null) {
                    TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] Executing request [{request}] with no participant");

                    _scope.SetNoAuthenticationInfo();
                }
                else {
                    TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] Executing request [{request}] with participant {participantName}");

                    var participantInfo = GetParticipatorInfo(participantName);
                    _scope.SetAuthenticationInfo(new CurrentParticipantModel(participantInfo.Id, participantInfo.Name, participantInfo.Color.HexString, participantInfo.IsFacilitator));
                }

                try {
                    await _scope.Send(request, CancellationToken.None);
                    await responseProcessor.Invoke();
                }
                catch (Exception ex) {
                    throw new InvalidOperationException($"Action failed [{request}] with participant {participantName}: {ex.Message}", ex);
                }
            });

            return this;
        }
        private TestCaseBuilder EnqueueMediatorAction(string participantName, Func<IRequest> requestFunc, Action responseProcessor) =>
            EnqueueMediatorAction(participantName, requestFunc, () => {
                responseProcessor.Invoke();
                return Task.CompletedTask;
            });
        private TestCaseBuilder EnqueueMediatorAction(Func<IRequest> requestFunc, Func<Task> responseProcessor) => EnqueueMediatorAction(null, requestFunc, responseProcessor);
        private TestCaseBuilder EnqueueMediatorAction(Func<IRequest> requestFunc, Action responseProcessor) =>
            EnqueueMediatorAction(requestFunc, () => {
                responseProcessor.Invoke();
                return Task.CompletedTask;
            });

        private TestCaseBuilder EnqueueMediatorAction<TResponse>(string participantName, Func<IRequest<TResponse>> requestFunc, Func<TResponse, Task> responseProcessor) {
            _actions.Enqueue(async () => {
                var request = requestFunc();

                if (participantName == null) {
                    TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] Executing request [{request}] with no participant");

                    _scope.SetNoAuthenticationInfo();
                }
                else {
                    TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] Executing request [{request}] with participant {participantName}");

                    var participantInfo = GetParticipatorInfo(participantName);
                    _scope.SetAuthenticationInfo(new CurrentParticipantModel(participantInfo.Id, participantInfo.Name, participantInfo.Color.HexString, participantInfo.IsFacilitator));
                }

                try {
                    var response = await _scope.Send(request, CancellationToken.None);
                    await responseProcessor.Invoke(response);
                }
                catch (Exception ex) {
                    throw new InvalidOperationException($"Action failed [{request}] with participant {participantName}: {ex.Message}", ex);
                }
            });

            return this;
        }
        private TestCaseBuilder EnqueueMediatorAction<TResponse>(string participantName, Func<IRequest<TResponse>> requestFunc, Action<TResponse> responseProcessor) =>
            EnqueueMediatorAction(participantName, requestFunc, r => {
                responseProcessor.Invoke(r);
                return Task.CompletedTask;
            });
        private TestCaseBuilder EnqueueMediatorAction<TResponse>(Func<IRequest<TResponse>> requestFunc, Func<TResponse, Task> responseProcessor) => EnqueueMediatorAction<TResponse>(null, requestFunc, responseProcessor);
        private TestCaseBuilder EnqueueMediatorAction<TResponse>(Func<IRequest<TResponse>> requestFunc, Action<TResponse> responseProcessor) =>
            EnqueueMediatorAction(requestFunc, r => {
                responseProcessor.Invoke(r);
                return Task.CompletedTask;
            });

        private sealed class FriendlyIdEntityDictionary {
            private readonly Dictionary<string, (Type Type, int Id)> _dataStore;

            public FriendlyIdEntityDictionary() {
                _dataStore = new Dictionary<string, (Type, int)>(StringComparer.Ordinal);
            }

            public int Get(string friendlyId, Type type) {
                if (!_dataStore.TryGetValue(friendlyId, out var item)) {
                    throw new ArgumentException($"Entity {type} with id '{friendlyId}' is not found");
                }

                return item.Id;
            }

            public void Set(string friendlyId, Type type, int itemId) {
                try {
                    _dataStore[friendlyId] = (type, itemId);
                }
                catch (ArgumentException) {
                    throw new ArgumentException($"Entity {type} with id '{friendlyId}' is already exists");
                }
            }
        }
    }
}
