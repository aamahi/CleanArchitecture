﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EmployeeManagement.Application.CacheRepositories;
using EmployeeManagement.Application.Dtos.EmployeeDtos;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Domain.Entities;
using TanvirArjel.EFCore.GenericRepository;

namespace EmployeeManagement.Application.Implementations.Services
{
    internal class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeCacheRepository _employeeCacheRepository;
        private readonly IRepository _repository;

        public EmployeeService(IEmployeeCacheRepository employeeCacheRepository, IRepository repository)
        {
            _employeeCacheRepository = employeeCacheRepository;
            _repository = repository;
        }

        public async Task<PaginatedList<EmployeeDetailsDto>> GetEmployeeListAsync(int pageNumber, int pageSize)
        {
            Expression<Func<Employee, EmployeeDetailsDto>> selectExpression = e => new EmployeeDetailsDto
            {
                EmployeeId = e.Id,
                EmployeeName = e.Name,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department.Name,
                DateOfBirth = e.DateOfBirth,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                IsActive = e.IsActive,
                CreatedAtUtc = e.CreatedAtUtc,
                LastModifiedAtUtc = e.LastModifiedAtUtc
            };

            PaginationSpecification<Employee> paginationSpecification = new PaginationSpecification<Employee>
            {
                PageIndex = pageNumber,
                PageSize = pageSize
            };

            PaginatedList<EmployeeDetailsDto> employeeDetailsDtos = await _repository.GetPaginatedListAsync(paginationSpecification, selectExpression);

            return employeeDetailsDtos;
        }

        public async Task<EmployeeDetailsDto> GetEmployeeDetailsAsync(int employeeId)
        {
            EmployeeDetailsDto employeeDetailsDto = await _employeeCacheRepository.GetDetailsByIdAsync(employeeId);

            return employeeDetailsDto;
        }

        public async Task CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto)
        {
            if (createEmployeeDto == null)
            {
                throw new ArgumentNullException(nameof(createEmployeeDto));
            }

            Employee employeeToBeCreated = new Employee()
            {
                Name = createEmployeeDto.EmployeeName,
                DepartmentId = createEmployeeDto.DepartmentId,
                DateOfBirth = createEmployeeDto.DateOfBirth,
                Email = createEmployeeDto.Email,
                PhoneNumber = createEmployeeDto.PhoneNumber
            };

            await _repository.InsertAsync(employeeToBeCreated);
        }

        public async Task UpdateEmplyeeAsync(UpdateEmployeeDto updateEmployeeDto)
        {
            try
            {
                if (updateEmployeeDto == null)
                {
                    throw new ArgumentNullException(nameof(updateEmployeeDto));
                }

                Employee employeeeToBeUpdated = await _repository.GetByIdAsync<Employee>(updateEmployeeDto.EmployeeId);

                if (employeeeToBeUpdated == null)
                {
                    throw new EntityNotFoundException(typeof(Employee), updateEmployeeDto.EmployeeId);
                }

                employeeeToBeUpdated.Name = updateEmployeeDto.EmployeeName;
                employeeeToBeUpdated.DepartmentId = updateEmployeeDto.DepartmentId;
                employeeeToBeUpdated.DateOfBirth = updateEmployeeDto.DateOfBirth;
                employeeeToBeUpdated.Email = updateEmployeeDto.Email;
                employeeeToBeUpdated.PhoneNumber = updateEmployeeDto.PhoneNumber;

                await _repository.UpdateAsync(employeeeToBeUpdated);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteEmployee(int employeeId)
        {
            Employee employeeeToBeDeleted = await _repository.GetByIdAsync<Employee>(employeeId);

            if (employeeeToBeDeleted == null)
            {
                throw new EntityNotFoundException(typeof(Employee), employeeId);
            }

            await _repository.DeleteAsync(employeeeToBeDeleted);
        }
    }
}
