﻿using Application.Handlers;
using Community.Domain.Contracts;
using Infrastructure;
using MediatR;

namespace Community.API.Application.Commands.Handlers {
    public class DeleteUserProfileCommandHandler : ICommandHandler<DeleteUserProfileCommand> {

        private readonly IUserProfileRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteUserProfileCommandHandler> _logger;

        public DeleteUserProfileCommandHandler (
            IUserProfileRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<DeleteUserProfileCommandHandler> logger) {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle (DeleteUserProfileCommand request, CancellationToken cancellationToken) {
            await _unitOfWork.ExecuteOptimisticUpdateAsync(async () => {
                var userProfile = await _repository.GetUserProfileAsync(request.Id, cancellationToken);

                if (userProfile != null) {
                    await _repository.DeleteUserProfileAsync(userProfile);
                    await _unitOfWork.CommitAsync(cancellationToken);

                    _logger.LogInformation("User profile ({UserId}) is deleted", request.Id);
                } else {
                    _logger.LogWarning("User profile ({UserId}) not found", request.Id);
                }
            });

            return Unit.Value;
        }
    }
}
