using Microsoft.AspNetCore.Mvc;
using TestEMR.Models;
using TestEMR.Data;
using TestEMR.Filter;
using TestEMR.Entities;
using TestEMR.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using System.Linq.Expressions;

namespace TestEMR.Controllers
{
    /// <summary>
    /// Controller responsible for managing requisitionline-related operations in the API.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints for adding, retrieving, updating, and deleting requisitionline information.
    /// </remarks>
    [Route("api/requisitionline")]
    [Authorize]
    public class RequisitionLineController : ControllerBase
    {
        private readonly TestEMRContext _context;

        public RequisitionLineController(TestEMRContext context)
        {
            _context = context;
        }

        /// <summary>Adds a new requisitionline to the database</summary>
        /// <param name="model">The requisitionline data to be added</param>
        /// <returns>The result of the operation</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [UserAuthorize("RequisitionLine",Entitlements.Create)]
        public IActionResult Post([FromBody] RequisitionLine model)
        {
            _context.RequisitionLine.Add(model);
            this._context.SaveChanges();
            return Ok(new { model.Id });
        }

        /// <summary>Retrieves a list of requisitionlines based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of requisitionlines</returns>
        [HttpGet]
        [UserAuthorize("RequisitionLine",Entitlements.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        public IActionResult Get([FromQuery] string filters, string searchTerm, int pageNumber = 1, int pageSize = 10, string sortField = null, string sortOrder = "asc")
        {
            List<FilterCriteria> filterCriteria = null;
            if (pageSize < 1)
            {
                return BadRequest("Page size invalid.");
            }

            if (pageNumber < 1)
            {
                return BadRequest("Page mumber invalid.");
            }

            if (!string.IsNullOrEmpty(filters))
            {
                filterCriteria = JsonHelper.Deserialize<List<FilterCriteria>>(filters);
            }

            var query = _context.RequisitionLine.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<RequisitionLine>.ApplyFilter(query, filterCriteria, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(RequisitionLine), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<RequisitionLine, object>>(Expression.Convert(property, typeof(object)), parameter);
                if (sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.OrderBy(lambda);
                }
                else if (sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.OrderByDescending(lambda);
                }
                else
                {
                    return BadRequest("Invalid sort order. Use 'asc' or 'desc'.");
                }
            }

            var paginatedResult = result.Skip(skip).Take(pageSize).ToList();
            return Ok(paginatedResult);
        }

        /// <summary>Retrieves a specific requisitionline by its primary key</summary>
        /// <param name="id">The primary key of the requisitionline</param>
        /// <returns>The requisitionline data</returns>
        [HttpGet]
        [Route("{id:Guid}")]
        [UserAuthorize("RequisitionLine",Entitlements.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public IActionResult GetById([FromRoute] Guid id)
        {
            var entityData = _context.RequisitionLine.IncludeRelated().FirstOrDefault(entity => entity.Id == id);
            return Ok(entityData);
        }

        /// <summary>Deletes a specific requisitionline by its primary key</summary>
        /// <param name="id">The primary key of the requisitionline</param>
        /// <returns>The result of the operation</returns>
        [HttpDelete]
        [UserAuthorize("RequisitionLine",Entitlements.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        [Route("{id:Guid}")]
        public IActionResult DeleteById([FromRoute] Guid id)
        {
            var entityData = _context.RequisitionLine.IncludeRelated().FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                return NotFound();
            }

            _context.RequisitionLine.Remove(entityData);
            var status = this._context.SaveChanges();
            return Ok(new { status });
        }

        /// <summary>Updates a specific requisitionline by its primary key</summary>
        /// <param name="id">The primary key of the requisitionline</param>
        /// <param name="updatedEntity">The requisitionline data to be updated</param>
        /// <returns>The result of the operation</returns>
        [HttpPut]
        [UserAuthorize("RequisitionLine",Entitlements.Update)]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        public IActionResult UpdateById(Guid id, [FromBody] RequisitionLine updatedEntity)
        {
            if (id != updatedEntity.Id)
            {
                return BadRequest("Mismatched Id");
            }

            this._context.RequisitionLine.Update(updatedEntity);
            var status = this._context.SaveChanges();
            return Ok(new { status });
        }

        /// <summary>Updates a specific requisitionline by its primary key</summary>
        /// <param name="id">The primary key of the requisitionline</param>
        /// <param name="updatedEntity">The requisitionline data to be updated</param>
        /// <returns>The result of the operation</returns>
        [HttpPatch]
        [UserAuthorize("RequisitionLine",Entitlements.Update)]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        public IActionResult UpdateById(Guid id, [FromBody] JsonPatchDocument<RequisitionLine> updatedEntity)
        {
            if (updatedEntity == null)
                return BadRequest("Patch document is missing.");
            var existingEntity = this._context.RequisitionLine.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
                return NotFound();
            updatedEntity.ApplyTo(existingEntity, ModelState);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            this._context.RequisitionLine.Update(existingEntity);
            var status = this._context.SaveChanges();
            return Ok(new { status });
        }
    }
}