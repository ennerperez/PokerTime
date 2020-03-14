﻿// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : BaseDataSeeder.cs
//  Project         : PokerTime.Application
// ******************************************************************************

namespace PokerTime.Application.App.Commands.SeedBaseData {
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Abstractions;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    internal sealed class BaseDataSeeder {
        private readonly IPokerTimeDbContext _pokerTimeDbContext;

        public BaseDataSeeder(IPokerTimeDbContext pokerTimeDbContext) {
            this._pokerTimeDbContext = pokerTimeDbContext;
        }

        public async Task SeedAllAsync(CancellationToken cancellationToken) {
            await this.SeedPredefinedParticipantColor(cancellationToken);

            await this._pokerTimeDbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task SeedPredefinedParticipantColor(CancellationToken cancellationToken) {
            if (await this._pokerTimeDbContext.PredefinedParticipantColors.AnyAsync(cancellationToken)) {
                return;
            }

            // Seed note lanes
            this._pokerTimeDbContext.PredefinedParticipantColors.AddRange(
                new PredefinedParticipantColor("Driver red", Color.Red),
                new PredefinedParticipantColor("Analytic blue", Color.Blue),
                new PredefinedParticipantColor("Amiable green", Color.Green),
                new PredefinedParticipantColor("Expressive yellow", Color.Yellow),
                new PredefinedParticipantColor("Juicy orange", Color.DarkOrange),
                new PredefinedParticipantColor("Participator purple", Color.Purple),
                new PredefinedParticipantColor("Boring blue-gray", Color.DarkSlateGray),
                new PredefinedParticipantColor("Adapting aquatic", Color.DodgerBlue),
                new PredefinedParticipantColor("Fresh lime", Color.Lime),
                new PredefinedParticipantColor("Tomàto tomató", Color.Tomato),
                new PredefinedParticipantColor("Goldie the bird", Color.Gold),
                new PredefinedParticipantColor("Farmer wheat", Color.Wheat)
            );
        }
    }
}
