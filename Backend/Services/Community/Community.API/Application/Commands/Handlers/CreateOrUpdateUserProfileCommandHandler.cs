﻿using Application.Handlers;
using Community.Domain.Contracts;
using Community.Domain.Models;
using Infrastructure;
using MediatR;
using SharedKernel.Exceptions;

namespace Community.API.Application.Commands.Handlers {
    public class CreateOrUpdateUserProfileCommandHandler : ICommandHandler<CreateOrUpdateUserProfileCommand> {

        private readonly IUserProfileRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateOrUpdateUserProfileCommandHandler> _logger;

        public CreateOrUpdateUserProfileCommandHandler (
            IUserProfileRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<CreateOrUpdateUserProfileCommandHandler> logger) {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle (CreateOrUpdateUserProfileCommand request, CancellationToken cancellationToken) {
            bool create = false;

            if (request.UpdateIfExists) {
                try {
                    await _unitOfWork.ExecuteOptimisticUpdateAsync(async () => {
                        var userProfile = await _repository.GetUserProfileAsync(request.Id);

                        if (userProfile != null) {
                            if (userProfile.Update(request.DisplayName, request.Handle, request.ThumbnailUrl, request.Version)) {
                                userProfile.IncrementVersion();
                                await _unitOfWork.CommitAsync(cancellationToken);
                                _logger.LogInformation("User profile ({UserId}) is updated", request.Id);
                            }
                        } else {
                            create = true;
                        }
                    });
                } catch (Exception ex) {
                    _logger.LogError(ex, "An error occurred when updating user profile ({UserId})", request.Id);
                    throw;
                }
            } else {
                create = true;
            }

            if (create) {
                try {
                    var userProfile = UserProfile.Create(request.Id, request.DisplayName, request.Handle, request.ThumbnailUrl, request.Version);
                    await _repository.AddUserProfileAsync(userProfile);
                    await _unitOfWork.CommitAsync(cancellationToken);
                    _logger.LogInformation("User profile ({UserId}) is created", request.Id);
                } catch (Exception ex) {
                    if (ex.Identify(ExceptionCategories.UniqueViolation)) {
                        _logger.LogWarning("User profile ({UserId}) already exists", request.Id);
                    } else {
                        _logger.LogError(ex, "An error occurred when adding user profile ({UserId})", request.Id);
                        throw;
                    }
                }
            }

            return Unit.Value;
        }
    }
}
