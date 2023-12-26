using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace PWTrilogyPatcher.Services;

public interface IStorageService
{
    public Task<IStorageFolder?> OpenFolderAsync();
}
