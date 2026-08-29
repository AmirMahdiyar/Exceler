using Exceler.Abstractions;
using TestExcelerWebApi.Dtos;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Processor
{
    public class DocumentProcessor : IExcelProcessor<DocumentExcelInput, DocumentDto>
    {
        public DocumentDto Process(DocumentExcelInput input)
        {
            return new DocumentDto
            {
                DocumentId = input.Id,
                DocumentType = input.Type,
                WarehouseName = input.Warehouse,
                Date = input.Date,
                Status = input.Status,
                FullDescription = $"invoice number :{input.Number} - {input.Description}"
            };
        }
    }
}
