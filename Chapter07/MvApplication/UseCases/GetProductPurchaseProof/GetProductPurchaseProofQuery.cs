using MediatR;

namespace MvApplication.UseCases.GetProductPurchaseProof;

public record GetProductPurchaseProofQuery(Guid ProductId) : IRequest<GetProductPurchaseProofResult>;
