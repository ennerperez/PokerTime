namespace PokerTime.Application.Estimations.Queries {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Common;
    using Common.Abstractions;
    using Common.Models;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public sealed class GetEstimationsQueryHandler : IRequestHandler<GetEstimationsQuery, GetEstimationsQueryResponse> {
        private readonly IPokerTimeDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;

        public GetEstimationsQueryHandler(IPokerTimeDbContextFactory dbContextFactory, IMapper mapper) {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
        }

        public async Task<GetEstimationsQueryResponse> Handle(GetEstimationsQuery request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using var dbContext = _dbContextFactory.CreateForEditContext();

            var userStory = await dbContext.UserStories.
                Where(x => x != null && x.Session.UrlId.StringId == request.SessionId && x.Id == request.UserStoryId).
                FirstOrDefaultAsync(cancellationToken);

            if (userStory == null) {
                throw new NotFoundException(nameof(UserStory), request.UserStoryId);
            }

            var estimations = await
                dbContext.Estimations
                    .Include(x => x.Participant)
                    .Include(x => x.Symbol)
                    .Where(x => x.UserStoryId == userStory.Id)
                    .ToListAsync(cancellationToken);

            var response = new GetEstimationsQueryResponse(
                _mapper.Map<List<EstimationModel>>(estimations)
            );

            return response;
        }
    }
}
