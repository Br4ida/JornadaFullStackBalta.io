using Fina.Api.Data;
using Fina.Core.Common;
using Fina.Core.Enums;
using Fina.Core.Handlers;
using Fina.Core.Models;
using Fina.Core.Requests.Transactions;
using Fina.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fina.Api.Handlers;

public class TransactionHandler(AppDbContext context) : ITransactionHandler
{
    public async Task<Response<Transaction?>> CreateAsync(CreateTransactionRequest request)
    {
        if (request is { Type: ETransactionType.Withdraw, Amount: >= 0 }) request.Amount *= -1;

        try
        {
            var transaction = new Transaction
            {
                UserId = request.UserId,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.Now,
                Amount = request.Amount,
                PaidOrReceivedAt = request.PaidOrReceivedAt,
                Title = request.Title,
                Type = request.Type,
            };

            await context.Transactions.AddAsync(transaction);
            await context.SaveChangesAsync();

            return new Response<Transaction?>
                (data: transaction, code: StatusCodes.Status201Created, message: "Transação criada com sucesso");
        }
        catch (Exception)
        {

            return new Response<Transaction?>
                (data: null, code: StatusCodes.Status500InternalServerError, message: "Não foi possível criar a transação");
        }
    }

    public async Task<Response<Transaction?>> DeleteAsync(DeleteTransactionRequest request)
    {
        try
        {
            var transaction = await context.Transactions
                                           .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (transaction is null)
                return new Response<Transaction?>
                    (data: null, code: StatusCodes.Status404NotFound, message: "Transação não encontrada");

            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();

            return new Response<Transaction?>(transaction);
        }
        catch (Exception)
        {

            return new Response<Transaction?>
                (data: null, code: StatusCodes.Status500InternalServerError, message: "Não foi possível deletar a transação");
        }
    }

    public async Task<Response<Transaction?>> GetByIdAsync(GetTransactionByIdRequest request)
    {
        try
        {
            var transaction = await context.Transactions
                                           .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            return transaction is null
                ? new Response<Transaction?>(data: null, code: StatusCodes.Status404NotFound, message: "Transação não encontrada")
                : new Response<Transaction?>(transaction);
        }
        catch (Exception)
        {
            return new Response<Transaction?>
                (data: null, code: StatusCodes.Status500InternalServerError, message: "Não foi possível recuperar a transação");
        }
    }

    public async Task<PagedResponse<List<Transaction>?>> GetByPeriodAsync(GetTransactionsByPeriodRequest request)
    {
        try
        {
            request.StartDate ??= DateTime.Now.GetFirstDay();
            request.EndDate ??= DateTime.Now.GetLastDay();
        }
        catch (Exception)
        {
            return new PagedResponse<List<Transaction>?>
                (data: null, code: StatusCodes.Status400BadRequest, message: "Não foi possível definir a data de início e término.");
        }

        var query = context.Transactions
                           .AsNoTracking()
                           .Where(x => x.UserId == request.UserId &&
                                  x.PaidOrReceivedAt >= request.StartDate &&
                                  x.PaidOrReceivedAt <= request.EndDate)
                           .OrderBy(x => x.PaidOrReceivedAt);

        var transaction = await query.Skip((request.PageNumber - 1) * request.PageSize)
                                    .Take(request.PageSize)
                                    .ToListAsync();

        var count = await query.CountAsync();

        return new PagedResponse<List<Transaction>?>(
            transaction,
            count,
            request.PageNumber,
            request.PageSize);
    }

    public async Task<Response<Transaction?>> UpdateAsync(UpdateTransactionRequest request)
    {
        if (request is { Type: ETransactionType.Withdraw, Amount: >= 0 }) request.Amount *= -1;

        try
        {
            var transaction = await context.Transactions
                                           .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (transaction is null)
                return new Response<Transaction?>
                    (data: null, code: StatusCodes.Status404NotFound, message: "Transação não encontrada");

            transaction.CategoryId = request.CategoryId;
            transaction.Amount = request.Amount;
            transaction.Title = request.Title;
            transaction.Type = request.Type;
            transaction.PaidOrReceivedAt = request.PaidOrReceivedAt;

            context.Transactions.Update(transaction);
            await context.SaveChangesAsync();

            return new Response<Transaction?>(transaction);
        }
        catch (Exception)
        {

            return new Response<Transaction?>
                (data: null, code: StatusCodes.Status500InternalServerError, message: "Não foi possível atualizar a transação");
        }
    }
}
