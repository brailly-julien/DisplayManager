using DisplayManager.Applications.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayManager.Tests.UnitTests;
public class ScreenManagementServiceTests
{
    [Fact]
    public void DetectConnectedScreens_ShouldNotThrowException()
    {
        // Arrange
        var screenManagementService = new ScreenManagementService();

        // Act
        var exception = Record.Exception(() => screenManagementService.DetectConnectedScreens());

        // Assert
        Assert.Null(exception);
    }

    // Ajoutez d'autres tests unitaires pour les méthodes restantes
}
