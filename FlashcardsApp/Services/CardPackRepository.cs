using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

using StudyApp.Data;
using StudyApp.Models;


namespace StudyApp.Services
{
    // load packs from file, edit, save or delete
    public static class CardPackRepository
    {
        // load packs from file
        private static List<CardPack> LoadPacksFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<CardPack>();
            }

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<CardPack>>(json);
        }

        // get current in-memory cardpacks
        public static List<CardPack> GetCardPacks(string filePath)
        {
            return LoadPacksFromFile(filePath);
        }

        // get a copy of card with corresponding ID 
        public static CardPack GetCardPackById(int packID, string filePath)
        {
            var cardPack = LoadPacksFromFile(filePath).FirstOrDefault(x => x.Id == packID);
            if (cardPack != null)
            {
                return new CardPack
                {
                    Id = cardPack.Id,
                    Name = cardPack.Name,
                    Storage = cardPack.Storage,
                    Description = cardPack.Description,
                    Language = cardPack.Language
                };
            }
            return new CardPack {};
        }

        // save CardPack changes 
        public static void UpdateCardPack(int cardId, CardPack cardPackUpdated, string filePath)
        {
            if (cardId != cardPackUpdated.Id) return;

            List<CardPack> cadPacks = LoadPacksFromFile(filePath);

            var cadPack = cadPacks.FirstOrDefault(x => x.Id == cardId);
            if (cadPack != null)
            {
                cadPack.Name = cardPackUpdated.Name;
                cadPack.Description = cardPackUpdated.Description;
                cadPack.Language = cardPackUpdated.Language;
                SaveCardPacksToFile(cadPacks, filePath);
            }
        }

        public static int SaveNewCardPack(CardPack newCardPack)
        {
            if (newCardPack != null)
            {
                string filePath1 = Path.Combine(CurrentData.Instance.CardPackPath, Globals.CardPacksFile);
                List<CardPack> cardPacks = LoadPacksFromFile(filePath1);
                int newMaxId = (cardPacks.Any() ? cardPacks.Max(cp => cp.Id) : 0) + 1;

                CardPack newPack = new CardPack
                {
                    Id = newMaxId,
                    Storage = newMaxId.ToString() + ".json",
                    Name = newCardPack.Name,
                    Description = newCardPack.Description
                };

                // TODO check if the cardPack already exists

                cardPacks.Add(newPack);
                SaveCardPacksToFile(cardPacks, filePath1);

                // New .json file
                string filePath2 = Path.Combine(CurrentData.Instance.CardPackPath, newPack.Storage);
                File.WriteAllText(filePath2, "[]");
                return newMaxId;
            }
            return -1;
        }

        public static void SaveNewStudySet(CardPack newStudySet)
        {
            if (newStudySet != null)
            {
                string filePath1 = Path.Combine(Globals.DataFolder, Globals.StudySetsFile);
                List<CardPack> studySets = LoadPacksFromFile(filePath1);
                int newMaxId = (studySets.Any() ? studySets.Max(cp => cp.Id) : 0) + 1;

                CardPack newPack = new CardPack
                {
                    Id = newMaxId,
                    Storage = newMaxId.ToString(),
                    Name = newStudySet.Name,
                    Description = newStudySet.Description,
                    Language = newStudySet.Language
                };

                // TODO check if the studySet already exists

                studySets.Add(newPack);
                SaveCardPacksToFile(studySets, filePath1);

                // New .json file
                string filePath2 = Path.Combine(Globals.DataFolder, newPack.Storage);
                Directory.CreateDirectory(filePath2);
                filePath2 = Path.Combine(filePath2, Globals.CardPacksFile);
                File.WriteAllText(filePath2, "[]");
            }
        }

        static void SaveCardPacksToFile(List<CardPack> flashcards, string filePath)
        {
            string json = JsonConvert.SerializeObject(flashcards, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        // detele a flashcard
        public static void DeleteCardPack(int packID, string filePath)
        {
            List<CardPack> cardPacks = LoadPacksFromFile(filePath);

            var cardPackToDelete = cardPacks.FirstOrDefault(f => f.Id == packID);
            if (cardPackToDelete != null)
            {
                cardPacks.Remove(cardPackToDelete);
                SaveCardPacksToFile(cardPacks, filePath);
            }
        }

        private static async Task<string> PrepareZipAsync()
        {
            List<Flashcard> FlashcardsList = FlashcardRepository.GetFlashcards();
            var imagePaths = FlashcardsList
                .SelectMany(fc => new[] { fc.Img1Name, fc.Img2Name })            // flatten both paths
                .Where(path => !string.IsNullOrWhiteSpace(path))                // ignore null/empty
                .Select(path => path!)                                          // null-forgiving
                .Distinct()
            .Where(File.Exists)
            .ToList();

            string tempZipPath = Path.Combine(FileSystem.CacheDirectory,
                                 $"sharedFlashcards_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

            using (var zipStream = new FileStream(tempZipPath, FileMode.Create))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                // Add JSON
                string jsonFileName = Path.GetFileName(CurrentData.Instance.FlashcardPath);
                var jsonEntry = archive.CreateEntry(jsonFileName);

                using (var input = File.OpenRead(CurrentData.Instance.FlashcardPath))
                using (var entryStream = jsonEntry.Open())
                {
                    await input.CopyToAsync(entryStream);
                }

                // Add each image
                foreach (var imagePath in imagePaths)
                {
                    var entry = archive.CreateEntry(Path.GetFileName(imagePath));
                    using (var imageStream = File.OpenRead(imagePath))
                    using (var entryStream = entry.Open())
                    {
                        await imageStream.CopyToAsync(entryStream);
                    }
                }
            }

            return tempZipPath;
        }

        public static async Task ShareCardPackAsync()
        {
            string zipPath = await PrepareZipAsync();
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Share zip with flashcards.",
                File = new ShareFile(zipPath)
            });
        }

        public static async Task ExportCardPackAsync()
        {
            try
            {
                string zipPath = await PrepareZipAsync(); // Generate zip file
                string zipFileName = Path.GetFileName(zipPath);

                // Open the zip as a stream for saving
                using var stream = File.OpenRead(zipPath);
                var fileSaverResult = await FileSaver.Default.SaveAsync(zipFileName, stream);

                if (fileSaverResult.IsSuccessful)
                {
                    await Toast.Make($"File saved at: {fileSaverResult.FilePath}").Show();
                }
                else
                {
                    await Toast.Make($"Error saving file: {fileSaverResult.Exception?.Message}").Show();
                }

                // delete temp zip after saving
                try { File.Delete(zipPath); } catch { }
            }
            catch (Exception ex)
            {
                await Toast.Make($"Error: {ex.Message}").Show();
            }
        }

        public static async Task AddCardPackFromZip(string Entry1, string Entry2)
        {
            var customZipFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/zip" } },
                { DevicePlatform.iOS, new[] { "com.pkware.zip-archive" } },
                { DevicePlatform.MacCatalyst, new[] { "com.pkware.zip-archive" } },
                { DevicePlatform.WinUI, new[] { ".zip" } }
            });

            var pickOptions = new PickOptions
            {
                PickerTitle = "Select ZIP file with flashcards",
                FileTypes = customZipFileType
            };

            var fileResult = await FilePicker.PickAsync(pickOptions);

            if (fileResult != null)
            {
                try
                {
                    using var zipStream = await fileResult.OpenReadAsync();

                    // 1. Extract ZIP to a temporary folder
                    string extractDir = Path.Combine(FileSystem.CacheDirectory, $"unpacked_{Guid.NewGuid()}");
                    Directory.CreateDirectory(extractDir);

                    using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            string fullPath = Path.Combine(extractDir, entry.FullName);
                            using var entryStream = entry.Open();
                            using var fileStream = File.Create(fullPath);
                            await entryStream.CopyToAsync(fileStream);
                        }
                    }

                    // 2. Find JSON file inside the unpacked folder
                    var jsonFilePath = Directory
                        .GetFiles(extractDir, "*.json")
                        .FirstOrDefault();

                    if (jsonFilePath == null)
                    {
                        await Toast.Make("No flashcard JSON found in ZIP").Show();
                        return;
                    }

                    // 3. Save the new card pack
                    CardPack cardPack = new CardPack { Name = Entry1, Description = Entry2 };
                    int newCardPackId = CardPackRepository.SaveNewCardPack(cardPack);
                    string jsonDestPath = Path.Combine(CurrentData.Instance.CardPackPath, $"{newCardPackId}.json");

                    // 4. Load flashcards, assign new IDs
                    List<Flashcard> newFlashcards = FlashcardRepository.LoadFlashcardsFromFile(jsonFilePath);
                    string packDir = Path.GetDirectoryName(jsonDestPath)!;

                    if (newFlashcards.Count > 0)
                    {
                        for (int i = 0; i < newFlashcards.Count; i++)
                        {
                            newFlashcards[i].Id = Guid.NewGuid();

                            // If image paths are included, copy the files and update paths
                            foreach (var imgProp in new[] { nameof(Flashcard.Img1Name), nameof(Flashcard.Img2Name) })
                            {
                                string? imgPath = typeof(Flashcard).GetProperty(imgProp)?.GetValue(newFlashcards[i]) as string;
                                if (!string.IsNullOrWhiteSpace(imgPath))
                                {
                                    string src = Path.Combine(extractDir, Path.GetFileName(imgPath));
                                    if (File.Exists(src))
                                    {
                                        string dest = Path.Combine(packDir, Path.GetFileName(src));
                                        File.Copy(src, dest, overwrite: true);
                                        typeof(Flashcard).GetProperty(imgProp)?.SetValue(newFlashcards[i], dest);
                                    }
                                }
                            }
                        }

                        FlashcardRepository.SaveFlashcardsToFile(newFlashcards, jsonDestPath);
                        await Toast.Make("Flashcards successfully added.").Show();
                    }
                    else
                    {
                        await Toast.Make("Empty pack of flashcards added.").Show();
                    }

                    // Delete temp file
                    try { Directory.Delete(extractDir, true); } catch { }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load ZIP: {ex.Message}");
                    await Toast.Make("Failed to import flashcards.").Show();
                }
            }
        }

        public static async Task CreateStarterPackAsync()
        {
            string rootFolder = Globals.DataFolder;
            Directory.CreateDirectory(rootFolder);

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "StudyApp.Resources.StarterPacks.zip";

            using Stream? zipStream = assembly.GetManifestResourceStream(resourceName);
            if (zipStream == null)
                throw new FileNotFoundException($"Embedded zip not found: {resourceName}");

            using var memoryStream = new MemoryStream();
            await zipStream.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                // Skip directory entries
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                string destinationPath = Path.Combine(rootFolder, entry.FullName);

                // Create the target directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                // Copy file content
                using var entryStream = entry.Open();
                using var fileStream = File.Create(destinationPath);
                await entryStream.CopyToAsync(fileStream);
            }

            string flashcardsPath = Path.Combine(Globals.DataFolder, "2", "1.json");
            UpdateImagePaths(flashcardsPath);
            flashcardsPath = Path.Combine(Globals.DataFolder, "2", "2.json");
            UpdateImagePaths(flashcardsPath);
        }

        private static void UpdateImagePaths(string flashcardsPath)
        {
            List<Flashcard> flashcards = FlashcardRepository.LoadFlashcardsFromFile(flashcardsPath);
            foreach (Flashcard flashcard in flashcards)
            {
                if (!string.IsNullOrEmpty(flashcard.Img1Name))
                {
                    if (DeviceInfo.Platform == DevicePlatform.Android ||
                        DeviceInfo.Platform == DevicePlatform.iOS ||
                        DeviceInfo.Platform == DevicePlatform.MacCatalyst)
                    {
                        flashcard.Img1Name = flashcard.Img1Name.Replace('\\', '/');
                    }
                    flashcard.Img1Name = Path.Combine(Globals.DataFolder, flashcard.Img1Name);
                }

                if (!string.IsNullOrEmpty(flashcard.Img2Name))
                {
                    if (DeviceInfo.Platform == DevicePlatform.Android ||
                        DeviceInfo.Platform == DevicePlatform.iOS ||
                        DeviceInfo.Platform == DevicePlatform.MacCatalyst)
                    {
                        flashcard.Img2Name = flashcard.Img2Name.Replace('\\', '/');
                    }
                    flashcard.Img2Name = Path.Combine(Globals.DataFolder, flashcard.Img2Name);

                }
            }
            FlashcardRepository.SaveFlashcardsToFile(flashcards, flashcardsPath);
        }

    }
}

