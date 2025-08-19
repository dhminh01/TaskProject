using System;
using MediatR;

namespace TaskService.Application.Tasks.Commands;

public record DeleteTaskCommand(Guid Id) : IRequest<bool>;
