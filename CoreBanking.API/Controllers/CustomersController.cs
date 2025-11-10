using AutoMapper;
using CoreBanking.API.Models;
using CoreBanking.API.Models.Requests;
using CoreBanking.APP.Customers.Commands.CreateCustomer;
using CoreBanking.APP.Customers.Queries.GetCustomerDetails;
using CoreBanking.APP.Customers.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoreBanking.API.Controllers;



[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(IMediator mediator, IMapper mapper, ILogger<CustomersController> logger)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerDto>>), StatusCodes.Status200OK)]
    public async
    Task<ActionResult<ApiResponse<List<CustomerDto>>>> GetCustomers()
    {
        var query = new GetCustomersQuery();
        var result = await _mediator.Send(query);

        return Ok(ApiResponse<List<CustomerDto>>.CreateSuccess(result.Data!));
    }

    [HttpGet("{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerDetailsDto>>> GetCustomer(Guid customerId)
    {
        var query = new GetCustomerDetailsQuery { CustomerId = customerId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<CustomerDetailsDto>.CreateSuccess(result.Data!));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        _logger.LogInformation("Received customer creation request for {Email}", request.Email);

        var command = new CreateCustomerCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            BVN = request.BVN,
            Address = request.Address,
            DateOfBirth = request.DateOfBirth
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully created customer with ID {CustomerId}", result.Data);
            return Ok(ApiResponse<CustomerId>.CreateSuccess(result.Data!, "Customer created successfully"));
        }

        _logger.LogWarning("Failed to create customer: {Error}", result.Errors);
        return BadRequest(ApiResponse<object>.CreateFailure(result.Errors));
    }
}
