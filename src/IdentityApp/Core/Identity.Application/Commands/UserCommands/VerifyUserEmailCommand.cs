﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using TanvirArjel.ArgumentChecker;
using TanvirArjel.EFCore.GenericRepository;

namespace Identity.Application.Commands.UserCommands
{
    public class VerifyUserEmailCommand : IRequest
    {
        public VerifyUserEmailCommand(string email, string code)
        {
            Email = email.ThrowIfNotValidEmail(nameof(email));
            Code = code.ThrowIfNullOrEmpty(nameof(code));
        }

        public string Email { get; }

        public string Code { get; }

        private class VerifyUserEmailCommandHandler : IRequestHandler<VerifyUserEmailCommand>
        {
            private readonly IRepository _repository;

            public VerifyUserEmailCommandHandler(IRepository repository)
            {
                _repository = repository;
            }

            public async Task<Unit> Handle(VerifyUserEmailCommand request, CancellationToken cancellationToken)
            {
                request.ThrowIfNull(nameof(request));

                IDbContextTransaction dbContextTransaction = await _repository.BeginTransactionAsync();

                try
                {
                    EmailVerificationCode emailVerificationCode = await _repository
                    .GetAsync<EmailVerificationCode>(evc => evc.Email == request.Email && evc.Code == request.Code && evc.UsedAtUtc == null);

                    if (emailVerificationCode == null)
                    {
                        throw new InvalidOperationException("Either email or password reset code is incorrect.");
                    }

                    if (DateTime.UtcNow > emailVerificationCode.SentAtUtc.AddMinutes(5))
                    {
                        throw new InvalidOperationException("The code is expired.");
                    }

                    User applicationUser = await _repository.GetAsync<User>(au => au.Email == request.Email);

                    if (applicationUser == null)
                    {
                        throw new InvalidOperationException("The provided email is not related to any account.");
                    }

                    applicationUser.EmailConfirmed = true;
                    await _repository.UpdateAsync(applicationUser);

                    emailVerificationCode.UsedAtUtc = DateTime.UtcNow;
                    await _repository.UpdateAsync(emailVerificationCode);

                    await dbContextTransaction.CommitAsync();

                    return Unit.Value;
                }
                catch (Exception)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
