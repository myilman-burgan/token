
using System.Dynamic;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token.Modules;

public static class CheckOtp
{
    public static void MapCheckOtpControlEndpoints(this WebApplication app)
    {
        app.MapPost("/amorphie-token-check-otp-login-flow", checkOtp)
        .Produces(StatusCodes.Status200OK);

        static async Task<IResult> checkOtp(
        [FromBody] dynamic body,
        [FromServices] IAuthorizationService authorizationService,
        IConfiguration configuration,
        DaprClient daprClient
        )
        {
            var transactionId = body.GetProperty("InstanceId").ToString();
            Console.WriteLine("check otp body:"+body.ToString());
            Console.WriteLine("check otp txn Id:"+transactionId);
            var providedCode = body.GetProperty("TRX-send-otp-login-flow");
            providedCode = providedCode.GetProperty("Data");
            providedCode = providedCode.GetProperty("entityData");
            providedCode = providedCode.GetProperty("otpValue");
            providedCode = providedCode.ToString();
            var generatedCode = await daprClient.GetStateAsync<string>(configuration["DAPR_STATE_STORE_NAME"],$"{transactionId}_Login_Otp_Code");
            
            if(providedCode == generatedCode)
            {
                dynamic variables = new ExpandoObject();
                variables.status = true;
                return Results.Ok(variables);
            }
            else
            {
                dynamic variables = new ExpandoObject();
                variables.status = false;
                variables.message = "Otp Check Failed";
                return Results.Ok(variables);
            }
        }

    }
}
