using ErrorOr;
using MapsterMapper;
using MediatR;
using OnlineVeterinary.Application.Common.Interfaces.Persistence;
using OnlineVeterinary.Application.Common.Interfaces.Services;
using OnlineVeterinary.Application.Features.Common;
using OnlineVeterinary.Domain.Pet.Entities;
using OnlineVeterinary.Domain.Reservation.Entities;
using OnlineVeterinary.Domain.Users.Entities;

namespace OnlineVeterinary.Application.Features.Reservations.Commands.Add
{
    public class AddReservationCommandHandler : IRequestHandler<AddReservationCommand, ErrorOr<ReservationDTO>>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPetRepository _petRepository;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICacheService _cacheService;

        public AddReservationCommandHandler(
                                    IReservationRepository reservationRepository,
                                    IMapper mapper,
                                    IUnitOfWork unitOfWork,
                                    IPetRepository petRepository,
                                    IDateTimeProvider dateTimeProvider,
                                    IUserRepository userRepository,
                                    ICacheService cacheService)
        {
            _reservationRepository = reservationRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _petRepository = petRepository;
            _dateTimeProvider = dateTimeProvider;
            _userRepository = userRepository;
            _cacheService = cacheService;
        }
        public async Task<ErrorOr<ReservationDTO>> Handle(
                                            AddReservationCommand request,
                                            CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync();
            var doctors = users.Where(a => a.Role.ToLower() == "doctor");
            var doctor = doctors.SingleOrDefault(a => a.Id == request.DoctorId);
            var pet = await _petRepository.GetByIdAsync(request.PetId);
            var myGuidId = Guid.Parse(request.CareGiverId);
            Reservation reservation;


            var careGiver = await _userRepository.GetByIdAsync(myGuidId);
            if (careGiver is null)
            {
                return Error.NotFound(description : "you have invalid Id or this user is not exist any more");
            }

            if (doctor is null)
            {
                return Error.NotFound(description : "the doctor with this id is not exist");
            }
            if (pet is null)
            {
                return Error.NotFound(description : "the pet with this id is not exist");
            }
            if (pet.CareGiverId != myGuidId)
            {
                return Error.NotFound(description : "you dont have any pet with this id");
            }

            var allReservations = await _reservationRepository.GetAllAsync();
            var doctorReservations = allReservations.Where(a => a.DoctorId == doctor.Id);
            var lastReserved = doctorReservations.OrderBy(a => a.DateOfReservation).LastOrDefault();

            var reserveDate = (lastReserved == null) ? _dateTimeProvider.UtcNow.AddMinutes(30)
                                : lastReserved.DateOfReservation.AddMinutes(30);

            if (IsInWorkingHours(reserveDate.TimeOfDay))
            {
                reservation = FormReservation(doctor, pet, myGuidId, reserveDate);
                _reservationRepository.Add(reservation);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<ReservationDTO>(reservation);
            }

            reserveDate = new DateTime(
                                reserveDate.Year,
                                reserveDate.Month,
                                reserveDate.Day + 1,
                                07, 0, 0);
            reservation = FormReservation(doctor, pet, myGuidId, reserveDate);
            _reservationRepository.Add(reservation);
            await _unitOfWork.SaveChangesAsync();
            _cacheService.RemoveData($"{myGuidId} reservations");

            return _mapper.Map<ReservationDTO>(reservation);


        }

        private static Reservation FormReservation(
            User doctor,
            Pet pet,
            Guid myGuidId,
            DateTime reserveDate)
        {
            return new Reservation()

            {
                DateOfReservation = reserveDate,
                CareGiverId = myGuidId,
                DoctorId = doctor.Id,
                PetId = pet.Id,
                DrLastName = doctor.LastName,
                PetName = pet.Name,
                CareGiverLastName = pet.CareGiverLastName


            };
        }

        private bool IsInWorkingHours(TimeSpan time)
        {
            return time >= WorkTime.Start.TimeOfDay &&
             time <= WorkTime.End.AddMinutes(-30).TimeOfDay;
        }


    }
}
