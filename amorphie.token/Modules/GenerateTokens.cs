
using System.Text.Json;
using amorphie.token.core.Models.Token;
using amorphie.token.core.Models;
using amorphie.token.Services.Authorization;
using Microsoft.AspNetCore.Mvc;
using amorphie.token.core.Helpers;
using System.Dynamic;
using amorphie.token.core.Extensions;
using amorphie.token.Services.TransactionHandler;

namespace amorphie.token.Modules;

public static class GenerateTokens
{
    public static void MapGenerateTokensControlEndpoints(this WebApplication app)
    {
        app.MapPost("/amorphie-token-generate-tokens", generateTokens)
        .Produces(StatusCodes.Status200OK);

        static async Task<IResult> generateTokens(
        [FromBody] dynamic body,
        [FromServices] ITokenService tokenService
        )
        {
            Console.WriteLine("GenerateTokens called");
            var transitionName = body.GetProperty("LastTransition").ToString();

            var dataBody = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");

            dynamic dataChanged = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(dataBody.ToString());

            dynamic targetObject = new System.Dynamic.ExpandoObject();

            targetObject.Data = dataChanged;

            var requestBodySerialized = body.GetProperty("TRX-start-password-flow-web").GetProperty("Data").GetProperty("entityData").ToString();

            TokenRequest requestBody = JsonSerializer.Deserialize<TokenRequest>(requestBodySerialized, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var clientInfoSerialized = body.GetProperty("clientSerialized").ToString();

            ClientResponse clientInfo = JsonSerializer.Deserialize<ClientResponse>(clientInfoSerialized, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var userInfoSerialized = body.GetProperty("userSerialized").ToString();

            LoginResponse userInfo = JsonSerializer.Deserialize<LoginResponse>(userInfoSerialized, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ServiceResponse<TokenResponse> result = await tokenService.GenerateTokenWithPasswordFromWorkflow(requestBody.MapTo<GenerateTokenRequest>(), clientInfo, userInfo, null);

            if (result.StatusCode == 200)
            {
                dataChanged.additionalData = result.Response;
                targetObject.Data = dataChanged;
                targetObject.TriggeredBy = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString());
                targetObject.TriggeredByBehalfOf = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString());
                dynamic variables = new Dictionary<string, dynamic>();
                variables.Add("status", true);
                variables.Add($"TRX{transitionName.ToString().Replace("-", "")}", targetObject);
                Console.WriteLine("GenerateTokens Success");
                return Results.Ok(variables);
            }
            else
            {
                dynamic variables = new ExpandoObject();
                variables.status = false;
                variables.tokenResponse = result.Detail;
                variables.LastTransition = "token-error";
                Console.WriteLine("GenerateTokens Error " + JsonSerializer.Serialize(variables));
                return Results.Ok(variables);
            }

        }

    }
}
