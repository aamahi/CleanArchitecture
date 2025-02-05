﻿using System;
using System.Threading.Tasks;
using EmployeeManagement.Application.Queries.DepartmentQueries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EmployeeManagement.Api.Endpoints.Departments
{
    public class GetDepartmentByIdEndpoint : DepartmentEndpointBase
    {
        private readonly IMediator _mediator;

        public GetDepartmentByIdEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{departmentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [SwaggerOperation(Summary = "Get the details of a department by department id.")]
        public async Task<ActionResult<DepartmentDetailsDto>> GetDepartment(Guid departmentId)
        {
            if (departmentId == Guid.Empty)
            {
                return BadRequest($"The value of {nameof(departmentId)} can't be empty.");
            }

            GetDepartmentByIdQuery query = new GetDepartmentByIdQuery(departmentId);

            DepartmentDetailsDto departmentDetailsDto = await _mediator.Send(query);
            return departmentDetailsDto;
        }
    }
}
