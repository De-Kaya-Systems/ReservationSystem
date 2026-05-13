using DeKayaServer.Contracts.Dashboard;
using DeKayaServer.Domain.CustomerBalance;
using DeKayaServer.Domain.PaymentHistory;
using DeKayaServer.Domain.Reservations;
using DeKayaServer.Domain.Reservations.Enum;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Dashboard;

public sealed record DashboardSummaryGetQuery
    : IRequest<Result<DashboardSummaryDto>>;

internal sealed class DashboardSummaryGetQueryHandler(
    ICustomerBalanceRepository customerBalanceRepository,
    IPaymentHistoryRepository paymentHistoryRepository,
    IReservationRepository reservationRepository )
    : IRequestHandler<DashboardSummaryGetQuery, Result<DashboardSummaryDto>>
{
    public async Task<Result<DashboardSummaryDto>> Handle(
        DashboardSummaryGetQuery request,
        CancellationToken cancellationToken )
    {
        var today = DateOnly.FromDateTime( DateTime.Today );

        var totalBilledAmount = await customerBalanceRepository
            .GetAll()
            .SumAsync( x => x.TotalAmount.Value, cancellationToken );

        var totalCollectedAmount = await paymentHistoryRepository
            .GetAll()
            .SumAsync( x => x.PaymentAmount, cancellationToken );

        var outstandingDebtAmount = await customerBalanceRepository
            .GetAll()
            .SumAsync( x => x.OutstandingAmount.Value, cancellationToken );

        var roomsAtCustomerCount = await reservationRepository
            .GetAll()
            .CountAsync(
                x => x.Status == ReservationStatus.DeliveredToCustomer,
                cancellationToken );

        var todayDeliveriesCount = await reservationRepository
            .GetAll()
            .CountAsync(
                x => x.DeliveryDate.Value == today,
                cancellationToken );

        var todayReturnsCount = await reservationRepository
            .GetAll()
            .CountAsync(
                x => x.PickUpDate.Value == today,
                cancellationToken );

        var dto = new DashboardSummaryDto
        {
            Finance = new DashboardFinanceSummaryDto
            {
                TotalBilledAmount = totalBilledAmount,
                TotalCollectedAmount = totalCollectedAmount,
                OutstandingDebtAmount = outstandingDebtAmount
            },
            CoolingRooms = new DashboardCoolingRoomSummaryDto
            {
                RoomsAtCustomerCount = roomsAtCustomerCount,
                TodayDeliveriesCount = todayDeliveriesCount,
                TodayReturnsCount = todayReturnsCount
            }
        };

        return Result<DashboardSummaryDto>.Succeed( dto );
    }
}
