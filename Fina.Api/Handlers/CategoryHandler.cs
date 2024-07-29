using Fina.Api.Data;
using Fina.Core.Handlers;
using Fina.Core.Models;
using Fina.Core.Requests.Categories;
using Fina.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fina.Api.Handlers;

public class CategoryHandler(AppDbContext context) : ICategoryHandler
{
    public async Task<Response<Category?>> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            UserId = request.UserId,
            Title = request.Title,
            Description = request.Description
        };

        try
        {
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            return new Response<Category?>
                (data: category, code: StatusCodes.Status201Created, message: "Categoria criada com sucesso");
        }
        catch
        {
            return new Response<Category?>
                (data: null, code: StatusCodes.Status500InternalServerError, message: "Não foi possível criar a categoria");
        }
    }

    public async Task<Response<Category?>> DeleteAsync(DeleteCategoryRequest request)
    {
        try
        {
            var category = await context.Categories
                                        .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (category is null)
                return new Response<Category?>
                    (data: null, code: StatusCodes.Status404NotFound, message: "Categoria não encontrada");

            context.Categories.Remove(category);
            await context.SaveChangesAsync();

            return new Response<Category?>
                (category, message: "Categoria excluída com sucesso");
        }
        catch (Exception)
        {

            return new Response<Category?>
                (data: null, code: StatusCodes.Status500InternalServerError, message: "Não foi possível deletar a categoria");
        }
    }

    public async Task<PagedResponse<List<Category>?>> GetAllAsync(GetAllCategoriesRequest request)
    {
        try
        {
            var query = context.Categories
                           .AsNoTracking()
                           .Where(x => x.UserId == request.UserId)
                           .OrderBy(x => x.Title);

            var categories = await query.Skip((request.PageNumber - 1) * request.PageSize)
                                        .Take(request.PageSize)
                                        .ToListAsync();

            var count = await query.CountAsync();

            return new PagedResponse<List<Category>?>(
                categories,
                count,
                request.PageNumber,
                request.PageSize);
        }
        catch (Exception)
        {

            return new PagedResponse<List<Category>?>
                (data: null, code: StatusCodes.Status500InternalServerError, message: "Não foi possível recuperar as categorias");
        }
        
    }

    public async Task<Response<Category?>> GetByIdAsync(GetCategoryByIdRequest request)
    {
        try
        {
            var category = await context.Categories
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            return category is null
                ? new Response<Category?>(data: null, code: StatusCodes.Status404NotFound, message: "Categoria não encontrada")
                : new Response<Category?>(category);
        }
        catch (Exception)
        {

            return new Response<Category?>
                (data: null, code: StatusCodes.Status500InternalServerError, message: "Não foi possível recuperar a categoria");
        }
    }

    public async Task<Response<Category?>> UpdateAsync(UpdateCategoryRequest request)
    {
        try
        {
            var category = await context.Categories
                                        .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

            if (category is null) 
                return new Response<Category?>
                    (data: null, code: StatusCodes.Status404NotFound, message: "Categoria não encontrada");

            category.Title = request.Title;
            category.Description = request.Description;

            context.Categories.Update(category);
            await context.SaveChangesAsync();

            return new Response<Category?>
                (category, message: "Categoria atualizada com sucesso");
        }
        catch (Exception)
        {

            return new Response<Category?>
                (data: null, code: StatusCodes.Status500InternalServerError, message: "Não foi possível atualizar a categoria");
        }
    }
}
