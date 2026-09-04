using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NbaQrApi.AzQr;
using NbaQrApi.Data;
using NbaQrApi.Models;
using NbaQrApi.Services;

namespace NbaQrApi.Controllers;

[ApiController]
[Route("qrapi")]
public sealed class QrApiController : ControllerBase
{
    private readonly ITerminalRepository _terminals;
    private readonly ITokenService _tokens;
    private readonly IQrPaymentService _payments;

    public QrApiController(ITerminalRepository terminals, ITokenService tokens, IQrPaymentService payments)
    {
        _terminals = terminals;
        _tokens = tokens;
        _payments = payments;
    }

    [HttpPost("authenticate")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TokenData>>> Authenticate(
        [FromBody] AuthenticateRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            return BadRequest(ApiResponse<TokenData>.Fail("serialNumber is required."));
        }

        var terminal = await _terminals.GetBySerialNumberAsync(request.SerialNumber, cancellationToken);
        if (terminal is null)
        {
            return Unauthorized(ApiResponse<TokenData>.Fail("Terminal was not found.", "Unauthorized"));
        }

        return Ok(ApiResponse<TokenData>.Ok(new TokenData { AccessToken = _tokens.CreateAccessToken(terminal) }));
    }

    [HttpGet("header/info")]
    [Authorize]
    public async Task<ActionResult<HeaderInfoResponse>> HeaderInfo(CancellationToken cancellationToken)
    {
        var terminal = await LoadTerminalAsync(cancellationToken);
        if (terminal is null)
        {
            return Unauthorized();
        }

        return Ok(new HeaderInfoResponse
        {
            SerialNumber = terminal.SerialNumber,
            RrnPrefix = [terminal.RrnPrefix],
            Register = new RegisterInfoDto
            {
                SerialNumber = terminal.SerialNumber,
                TerminalModel = terminal.TerminalModel,
                CountryCode = terminal.HeaderCountryCode,
                CurrencyCode = terminal.CurrencyCode,
                TerminalLanguageCode = terminal.TerminalLanguageCode,
                ReceiptLanguageCode = terminal.ReceiptLanguageCode,
                TimeZone = terminal.TimeZone
            },
            Company = new CompanyInfoDto
            {
                CompanyCode = terminal.CompanyCode,
                CompanyName = terminal.CompanyName
            },
            Merchant = new MerchantInfoDto
            {
                MerchantName = terminal.MerchantName,
                MerchantAddress1 = terminal.MerchantAddress1,
                PhoneNumber = terminal.PhoneNumber,
                CategoryCode = terminal.CategoryCode,
                MerchantNo = terminal.MerchantNo,
                Email = terminal.Email,
                TaxNumber = terminal.TaxNumber
            }
        });
    }

    [HttpPost("payment")]
    [Authorize]
    public async Task<ActionResult<PaymentResponse>> Payment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var serial = TokenSerial();
        if (string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            request.SerialNumber = serial ?? "";
        }

        if (serial is not null && !string.Equals(serial, request.SerialNumber, StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _payments.CreateAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (AzQrValidationException ex)
        {
            return BadRequest(ApiResponse<PaymentResponse>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<PaymentResponse>.Fail(ex.Message));
        }
    }

    [HttpGet("PaymentStatus/byid/{uniqueId}")]
    [Authorize]
    public async Task<ActionResult<PaymentResponse>> PaymentStatus(string uniqueId, CancellationToken cancellationToken)
    {
        var payment = await _payments.GetByUniqueIdAsync(uniqueId, includeQr: false, cancellationToken);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPut("cancel/{uniqueId}")]
    [Authorize]
    public async Task<ActionResult<PaymentResponse>> Cancel(string uniqueId, CancellationToken cancellationToken)
    {
        var payment = await _payments.CancelAsync(uniqueId, cancellationToken);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpGet("{uniqueId:minlength(6)}")]
    [Authorize]
    public async Task<ActionResult<PaymentResponse>> GetByUniqueId(string uniqueId, CancellationToken cancellationToken)
    {
        var payment = await _payments.GetByUniqueIdAsync(uniqueId, includeQr: true, cancellationToken);
        return payment is null ? NotFound() : Ok(payment);
    }

    private string? TokenSerial()
        => User.FindFirstValue("serialNumber");

    private async Task<Domain.Terminal?> LoadTerminalAsync(CancellationToken cancellationToken)
    {
        var serial = TokenSerial();
        if (string.IsNullOrWhiteSpace(serial))
        {
            return null;
        }

        return await _terminals.GetBySerialNumberAsync(serial, cancellationToken);
    }
}
