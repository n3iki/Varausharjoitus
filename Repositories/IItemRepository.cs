using Varausharjoitus.Models;

namespace Varausharjoitus.Repositories
{
    public interface IItemRepository
    {
        public Task<Item> GetItemAsync(long id); //get yhdelle itemille
        public Task<IEnumerable<Item>> GetItemsAsync(); //get kaikille itemeille
        public Task<IEnumerable<Item>> GetItemsAsync(User user); //get jossa username parametrina
        public Task<IEnumerable<Item>> QueryItems(String query); //get hakusanan sisältäville itemeille
        public Task<Item> AddItemAsync(Item item);
        public Task<Item> UpdateItemAsync(Item item);
        public Task<Boolean> DeleteItemAsync(Item item);
        public Task<Boolean> ClearImages(Item item); //kuvienpoistofunktio
    }
}
