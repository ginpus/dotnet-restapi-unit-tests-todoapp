using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Contracts.Enums;
using Contracts.Models.RequestModels;
using Contracts.Models.ResponseModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            List<TodoItemReadModel> todos)
        {
            // Arrange
            var userId = SetupHttpContext();

            _todosRepositoryMock
                .Setup(mock => mock.GetAllAsync(userId))
                .ReturnsAsync(todos);

            // Act
            var result = await _sut.GetAll();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(todos);

            _todosRepositoryMock.Verify(mock => mock.GetAllAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task GetTodo_When_AllChecks_Pass(
            Guid id,
            TodoItemReadModel todo)
        {
            // Arrange
            var userId = SetupHttpContext();

            _todosRepositoryMock
                .Setup(mock => mock.GetAsync(id, userId))
                .ReturnsAsync(todo);

            // Act
            var result = await _sut.Get(id);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(todo);

            _todosRepositoryMock.Verify(mock => mock.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task GetTodo_When_TodoId_IsNull(
           Guid id)
        {
            // Arrange
            var userId = SetupHttpContext();

            _todosRepositoryMock
                .Setup(mock => mock.GetAsync(id, userId))
                .ReturnsAsync((TodoItemReadModel)null);


            // Act
            var result = await _sut.Get(id);

            // Assert
            result
                .Result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().BeEquivalentTo($"Todo item with id: '{id}' does not exist");

            _todosRepositoryMock.Verify(mock => mock.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task Create_TodoItem_When_CreateTodoItemRequest_Received(CreateTodoItemRequest request)
        {
            // Arrange
            var userId = SetupHttpContext();

            var expectedResult = new TodosItemResponse
            {
                UserId = userId,
                Title = request.Title,
                Description = request.Description,
                Difficulty = request.Difficulty,
                IsDone = false
            };

            // Act
            var result = await _sut.Create(request);

            // Assert
            result
                .Result.Should().BeOfType<CreatedAtActionResult>()
                .Which.Value.Should().BeOfType<TodosItemResponse>()
                .Which.Should().BeEquivalentTo(expectedResult, options =>
                    options
                        .Excluding(model => model.Id)
                        .Excluding(model => model.DateCreated));

            _todosRepositoryMock.Verify(mock => mock.SaveOrUpdateAsync(It.Is<TodoItemReadModel>(model =>
                model.UserId == expectedResult.UserId &&
                model.Title == expectedResult.Title &&
                model.Description == expectedResult.Description &&
                model.Difficulty == expectedResult.Difficulty &&
                model.IsDone == expectedResult.IsDone)), Times.Once);
        }

        [Theory, AutoData]
        public async Task Update_Returns_NotFound_When_TodoItem_Is_Null(Guid id, UpdateTodoItemRequest request)
        {
            // Arrange
            var userId = SetupHttpContext();

            _todosRepositoryMock
                .Setup(mock => mock.GetAsync(id, userId))
                .ReturnsAsync((TodoItemReadModel)null);

            // Act
            var result = await _sut.Update(id, request);

            // Assert
            result
                .Result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().BeEquivalentTo($"Todo item with id: '{id}' does not exist");

            _todosRepositoryMock
                .Verify(mock => mock.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task Update_TodoItem_When_UpdateTodoItem_Request_Received(
            TodoItemReadModel todoItemReadModel,
            Guid id,
            UpdateTodoItemRequest request)
        {
            // Arrange
            var userId = SetupHttpContext();

            _todosRepositoryMock
                .Setup(mock => mock.GetAsync(id, userId))
                .ReturnsAsync(todoItemReadModel);

            var expectedResult = todoItemReadModel.MapToTodoItemResponse();
            expectedResult.Title = request.Title;
            expectedResult.Description = request.Description;

            // Act
            var result = await _sut.Update(id, request);

            // Assert
            result.Value.Should().BeEquivalentTo(expectedResult);

            _todosRepositoryMock
                .Verify(mock => mock.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);

            _todosRepositoryMock
                .Verify(mock => mock.SaveOrUpdateAsync(It.Is<TodoItemReadModel>(model =>
                    model.Id == expectedResult.Id &&
                    model.UserId == expectedResult.UserId &&
                    model.Title == expectedResult.Title &&
                    model.Description == expectedResult.Description &&
                    model.Difficulty == expectedResult.Difficulty &&
                    model.IsDone == expectedResult.IsDone &&
                    model.DateCreated == expectedResult.DateCreated)), Times.Once);
        }

        [Theory, AutoData]
        public async Task UpdateStatus_Returns_NotFound_When_TodoItem_Is_Null(Guid id)
        {
            // Arrange
            var userId = SetupHttpContext();

            _todosRepositoryMock
                .Setup(mock => mock.GetAsync(id, userId))
                .ReturnsAsync((TodoItemReadModel)null);

            // Act
            var result = await _sut.UpdateStatus(id);

            // Assert
            result
                .Result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().BeEquivalentTo($"Todo item with id: '{id}' does not exist");

            _todosRepositoryMock
                .Verify(mock => mock.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoData]
        public async Task UpdateStatus_When_TodoItem_Is_Found(TodoItemReadModel todoItemReadModel, Guid id)
        {
            // Arrange
            var userId = SetupHttpContext();

            _todosRepositoryMock
                .Setup(mock => mock.GetAsync(id, userId))
                .ReturnsAsync(todoItemReadModel);

            var expectedResult = todoItemReadModel.MapToTodoItemResponse();
            expectedResult.IsDone = !todoItemReadModel.IsDone;

            // Act
            var result = await _sut.UpdateStatus(id);

            // Assert
            result.Value.Should().BeEquivalentTo(expectedResult);

            _todosRepositoryMock
                .Verify(mock => mock.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);

            _todosRepositoryMock
                .Verify(mock => mock.SaveOrUpdateAsync(It.Is<TodoItemReadModel>(model =>
                    model.Id == expectedResult.Id &&
                    model.UserId == expectedResult.UserId &&
                    model.Title == expectedResult.Title &&
                    model.Description == expectedResult.Description &&
                    model.Difficulty == expectedResult.Difficulty &&
                    model.IsDone == expectedResult.IsDone &&
                    model.DateCreated == expectedResult.DateCreated)), Times.Once);
        }

        private Guid SetupHttpContext()
        {
            var userId = Guid.NewGuid();

            _httpContextMock
                .SetupGet(mock => mock.Items["userId"])
                .Returns(userId);

            return userId;
        }
    }
}