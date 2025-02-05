﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.Domain.Entities;
using MediatR;
using TanvirArjel.ArgumentChecker;
using TanvirArjel.EFCore.GenericRepository;

namespace Identity.Application.Commands.UserCommands
{
    public class UpdateLanguageCultureCommand : IRequest
    {
        public UpdateLanguageCultureCommand(Guid userId, string languageCulture)
        {
            UserId = userId.ThrowIfEmpty(nameof(userId));
            LanguageCulture = languageCulture.ThrowIfNullOrEmpty(nameof(languageCulture));
        }

        public Guid UserId { get; }

        public string LanguageCulture { get; }

        private class UpdateLanguageCultureCommandHandler : IRequestHandler<UpdateLanguageCultureCommand>
        {
            private readonly IRepository _repository;

            public UpdateLanguageCultureCommandHandler(IRepository repository)
            {
                _repository = repository;
            }

            public async Task<Unit> Handle(UpdateLanguageCultureCommand request, CancellationToken cancellationToken)
            {
                request.ThrowIfNull(nameof(request));

                User userToBeUpdated = await _repository.GetByIdAsync<User>(request.UserId);

                if (userToBeUpdated == null)
                {
                    throw new InvalidOperationException($"The ApplicationUser does not exist with id value: {request.UserId}.");
                }

                userToBeUpdated.LanguageCulture = request.LanguageCulture;
                await _repository.UpdateAsync(userToBeUpdated);

                return Unit.Value;
            }
        }
    }
}
