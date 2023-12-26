using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace PWTrilogyPatcher.Services;

public class StorageService : IStorageService
{
    private readonly Window _target;

    public StorageService(Window target)
    {
        _target = target;
    }

    public async Task<IStorageFolder?> OpenFolderAsync()
    {
        var folder = await _target.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Seleccionar la carpeta del juego",
            AllowMultiple = false
        });

        return folder.Count >= 1 ? folder[0] : null;
    }
}
