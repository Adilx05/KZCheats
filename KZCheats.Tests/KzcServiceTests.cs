using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using KZCheats.Core.Models;
using KZCheats.Core.Services;
using Xunit;

namespace KZCheats.Tests
{
    public class KzcServiceTests
    {
        [Fact]
        public void LoadKzcFile_ShouldReturnKzcFile_WhenValidFileIsProvided()
        {
            // Arrange
            var filePath = "validFile.kzc";
            var fileContent = JsonSerializer.Serialize(new KzcFile
            {
                GameName = "Test Game",
                GameVersion = "1.0",
                ProcessName = "test.exe",
                Cheats = new List<CheatEntry>
                {
                    new CheatEntry
                    {
                        Name = "Test Cheat",
                        Address = "0x123456",
                        Value = "100",
                        Type = "int"
                    }
                }
            });
            File.WriteAllText(filePath, fileContent);

            // Act
            var result = KzcService.LoadKzcFile(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Game", result.GameName);
            Assert.Equal("1.0", result.GameVersion);
            Assert.Equal("test.exe", result.ProcessName);
            Assert.Single(result.Cheats);
            Assert.Equal("Test Cheat", result.Cheats[0].Name);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public void LoadKzcFile_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            var filePath = "nonExistentFile.kzc";

            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() => KzcService.LoadKzcFile(filePath));
            Assert.Equal("KZC file not found.", exception.Message);
        }
    }
}
