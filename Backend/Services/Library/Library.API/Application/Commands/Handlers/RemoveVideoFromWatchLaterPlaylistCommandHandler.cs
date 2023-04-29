﻿using Application.Handlers;
using Infrastructure;
using Library.Domain.Contracts;
using Library.Domain.Models;
using MediatR;
using SharedKernel.Exceptions;

namespace Library.API.Application.Commands.Handlers {
    public class RemoveItemFromWatchLaterPlaylistCommandHandler : ICommandHandler<RemoveItemFromWatchLaterPlaylistCommand> {

        private readonly IUniquePlaylistRepository<WatchLaterPlaylist, OrderedPlaylistItem> _playlistRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveItemFromWatchLaterPlaylistCommandHandler (
            IUniquePlaylistRepository<WatchLaterPlaylist, OrderedPlaylistItem> playlistRepository,
            IUnitOfWork unitOfWork) {
            _playlistRepository = playlistRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle (RemoveItemFromWatchLaterPlaylistCommand request, CancellationToken cancellationToken) {
            await _unitOfWork.ExecuteTransactionAsync(async () => {
                var playlist = await _playlistRepository.GetPlaylistIncludingItem(request.UserId, request.ItemId, true, cancellationToken);

                if (playlist == null) {
                    throw new AppException("Playlist not found", null, StatusCodes.Status404NotFound);
                }

                playlist.RemoveItem(request.ItemId);
                await _unitOfWork.CommitAsync(cancellationToken);
            });

            return Unit.Value;
        }

    }
}
