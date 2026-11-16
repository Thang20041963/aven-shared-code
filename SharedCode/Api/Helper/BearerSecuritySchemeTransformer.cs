using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SharedCode.Api.Helper
{
    internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider schemeProvider)
        : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(OpenApiDocument document,
            OpenApiDocumentTransformerContext context, CancellationToken ct)
        {
            var schemes = await schemeProvider.GetAllSchemesAsync();
            if (!schemes.Any(s => s.Name == JwtBearerDefaults.AuthenticationScheme))
                return;

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };

            document.Security =
            [
                new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            }
            ];
        }
    }
}
