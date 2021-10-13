using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Contracts.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Persistence.Models.ReadModels;
using Persistence.Repositories;
using RestAPI.Controllers;
using Xunit;

namespace RestAPI.UnitTests.Controllers
{
    public class TodosController_Should
    {
        private readonly Mock<ITodosRepository> _todosRepositoryMock = new Mock<ITodosRepository>();
        private readonly Mock<IUserRepository> _usersRepositoryMock = new Mock<IUserRepository>();
        private readonly Mock<HttpContext> _httpContextMock = new Mock<HttpContext>();

        private readonly TodosController _sut;

        public TodosController_Should()
        {
            _sut = new TodosController(_todosRepositoryMock.Object, _usersRepositoryMock.Object)
            {
                ControllerContext =
                {
                    HttpContext = _httpContextMock.Object
                }
            };
        }

        [Theory, AutoData]
        public async Task GetAllTodos_When_GetAll_Is_Called(
            Guid userId,
            List<TodoItemReadModel> todos)
        {
            // Arrange
            _todosRepositoryMock
                .Setup(mock => mock.GetAllAsync(userId))
                .ReturnsAsync(todos);

            _httpContextMock
                .SetupGet(mock => mock.Items["userId"])
                .Returns(userId);

            // Act
            var result = await _sut.GetAll();

            // Assert
            result.Should().BeEquivalentTo(todos);

            _todosRepositoryMock.Verify(mock => mock.GetAllAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task GetTodo_When_AllChecks_Pass(
            Guid userId,
            Guid id,
            TodoItemReadModel todo)
        {
            // Arrange
            _todosRepositoryMock
                .Setup(mock => mock.GetAsync(id, userId))
                .ReturnsAsync(todo);

            _httpContextMock
                .SetupGet(mock => mock.Items["userId"])
                .Returns(userId);

            // Act
            var result = await _sut.Get(id);

            // Assert
            result.Value.Should().BeEquivalentTo(todo);

            _todosRepositoryMock.Verify(mock => mock.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task GetTodo_When_TodoId_IsNull(
           Guid userId,
           Guid id)
        {
            // Arrange
            _httpContextMock
                .SetupGet(mock => mock.Items["userId"])
                .Returns(userId);

            _todosRepositoryMock
                .Setup(mock => mock.GetAsync(id, userId))
                .ReturnsAsync((TodoItemReadModel)null);


            // Act

            var result = await _sut
                .Invoking(sut => sut.Get(id))
                .Should().ThrowAsync<Exception>()
                .WithMessage($"Todo item with id: '{id}' does not exist");

            // Assert

            _todosRepositoryMock.Verify(mock => mock.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }
    }
}