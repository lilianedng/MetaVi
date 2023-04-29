﻿using Application.Handlers;
using Infrastructure;
using Library.Domain.Contracts;
using Library.Domain.Models;
using MediatR;
using SharedKernel.Exceptions;

namespace Library.API.Application.Commands.Handlers {
    public class MovePlaylistItemCommandHandler : ICommandHandler<MovePlaylistItemCommand> {

        private readonly IPlaylistRepository<Playlist, OrderedPlaylistItem> _playlistRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MovePlaylistItemCommandHandler (IPlaylistRepository<Playlist, OrderedPlaylistItem> playlistRepository, IUnitOfWork unitOfWork) {
            _playlistRepository = playlistRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle (MovePlaylistItemCommand request, CancellationToken cancellationToken) {
            await _unitOfWork.ExecuteTransactionAsync(async () => {
                var playlist = await _playlistRepository.GetPlaylistIncludingItem(request.PlaylistId, request.ItemId, true, cancellationToken);

                if (playlist == null) {
                    throw new AppException("Playlist not found", null, StatusCodes.Status404NotFound);
                }

                if (!playlist.Items.Any(x => x.Id == request.ItemId)) {
                    throw new AppException("Item not found", null, StatusCodes.Status404NotFound);
                }

                if (playlist.UserId != request.UserId) {
                    throw new AppException("Only the owner can edit the playlist", null, StatusCodes.Status403Forbidden);
                }

                var fromPosition = playlist.Items.First(x => x.Id == request.ItemId).Position;
                var toPosition = request.ToPosition == -1 ? playlist.ItemsCount - 1 : request.ToPosition;

                playlist.MoveItem(fromPosition, toPosition);
                await _unitOfWork.CommitAsync(cancellationToken);
            });

            return Unit.Value;
        }

    }
}
