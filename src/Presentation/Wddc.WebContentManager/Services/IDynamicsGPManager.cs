using DynamicsGPServiceReference;

namespace Wddc.PurchasingOrderApp.Services
{
    public interface IDynamicsGPManager
    {
        DynamicsGPClient GetClient();
    }
}