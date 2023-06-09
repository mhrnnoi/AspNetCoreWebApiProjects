using ErrorOr;
using Moq;
using OnlineVeterinary.Application.Common.Interfaces.Persistence;
using OnlineVeterinary.Application.Features.Reservations.Commands.DeleteById;
using OnlineVeterinary.Domain.Reservation.Entities;
using OnlineVeterinary.Domain.Users.Entities;

namespace OnlineVeterinary.Application.UnitTests.Reservations.Commands;
public class DeleteReservationByIdCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Guid _userId;
    private readonly Guid _reservationId;
    private readonly DeleteReservationByIdCommand _command;
    private readonly DeleteReservationByIdCommandHandler _handler;
    private readonly Mock<ICacheService> _chacheServiceMock;

    public DeleteReservationByIdCommandHandlerTests()
    {
        _userRepositoryMock = new();
        _unitOfWorkMock = new();
        _chacheServiceMock = new();
        _reservationRepositoryMock = new();

        _userId = Guid.NewGuid();
        _reservationId = Guid.NewGuid();

        _command = new DeleteReservationByIdCommand(
                        _reservationId,
                        _userId.ToString(),
                        "0");
        _handler = new DeleteReservationByIdCommandHandler(
                        _reservationRepositoryMock.Object,
                        _unitOfWorkMock.Object,
                        _userRepositoryMock.Object,
                        _chacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenUserIsNotExist()
    {
        //Arrange

        _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId))
                            .ReturnsAsync((User?)null);

        //Act
        var result = await _handler.Handle(_command, default);
        //Assert
        Assert.True(result.IsError);
        Assert.Equal(Error.NotFound(description: "you have invalid Id or this user is not exist any more"),
                                     result.FirstError);

    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenNoReservationWithUserId()
    {
        //Arrange

        var user = new User();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId))
                            .ReturnsAsync(user);

        var reservations = new List<Reservation>();
        _reservationRepositoryMock.Setup(x => x.GetAllAsync())
                                    .ReturnsAsync(reservations);

        //Act
        var result = await _handler.Handle(_command, default);
        //Assert
        Assert.True(result.IsError);
        Assert.Equal(Error.NotFound(description: "you dont have any reservation with this id"),
                                    result.FirstError);

    }
    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenInputIsValid()
    {
        //Arrange


        var user = new User();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId))
                            .ReturnsAsync(user);

        var reservations = new List<Reservation>() { new Reservation() { DoctorId = _userId,
                                                                         Id = _command.Id } };

        _reservationRepositoryMock.Setup(x => x.GetAllAsync())
                                    .ReturnsAsync(reservations);

        _reservationRepositoryMock.Setup(x => x.Remove(It.IsAny<Reservation>()));

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync());

        var key = $"{_userId} reservations";

        _chacheServiceMock.Setup(x => x.RemoveData(key));

        //Act
        var result = await _handler.Handle(_command, default);
        //Assert
        Assert.False(result.IsError);
        Assert.Equal("Deleted successfully", result.Value);

    }
}
