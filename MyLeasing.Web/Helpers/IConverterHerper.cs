using MyLeasing.Web.Data.Entities;
using MyLeasing.Web.Models;
using System.Threading.Tasks;

namespace MyLeasing.Web.Helpers
{
    public interface IConverterHerper
    {
        Task<Property> ToPropertyAsync(PropertyViewModel model, bool isNew);
        PropertyViewModel ToPropertyViewModel(Property property);
        Task<Contract> ToContractAsync(ContractViewModel model, bool isNew);
        ContractViewModel ToContractViewModel(Contract contract);
    }
}