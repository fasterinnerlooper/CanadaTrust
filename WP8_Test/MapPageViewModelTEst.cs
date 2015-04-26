using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using CanadaTrustv1;
using CanadaTrustv1.ViewModel;
using Moq;
using System.Windows.Controls;

namespace WP8_Test
{
    [TestClass]
    public class MapPageViewModelTest
    {
        [TestMethod]
        public void ShouldSetBranchDisplaySizeWhenNoBranchesAreSet()
        {
            // Arrange
            var mapPage = new Mock<MapPage>();
            var sut = new MapPageViewModel(mapPage.Object);

            // Act
            sut.BranchDisplaySize = 10;

            // Assert
        }
    }
}
