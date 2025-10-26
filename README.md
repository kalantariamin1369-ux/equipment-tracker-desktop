# Equipment Tracker Desktop

A robust desktop application for managing equipment inventory, built with C# and Windows Forms.

## Features

- **Comprehensive Equipment Management**: Add, update, delete, and track equipment items
- **Quantity Tracking**: Easily add or remove quantities with transaction logging
- **Categorization**: Organize equipment by categories for better management
- **Low Stock Alerts**: Visual alerts for items running low on stock
- **Search & Filter**: Quick search by name or category filtering
- **Sortable Grid**: Sort equipment list by any column
- **In-place Editing**: Direct editing of equipment details in the grid
- **Transaction History**: Detailed log of all quantity changes with timestamps
- **SQLite Database**: Persistent lightweight database storage
- **CSV Import/Export**: Import equipment from CSV and export inventory data
- **Theming**: Light and Dark theme support
- **Database Backup**: Easy backup functionality for data protection
- **Error Logging**: Comprehensive error logging for debugging

## Screenshots

![Equipment Management Tab](https://via.placeholder.com/800x600/f0f0f0/333?text=Equipment+Management+Tab)

![Transaction History Tab](https://via.placeholder.com/800x600/f0f0f0/333?text=Transaction+History+Tab)

## System Requirements

- Windows 10 or later
- .NET Framework 4.8 (usually pre-installed)
- SQLite (embedded with application)
- Visual Studio 2019+ (for building from source)

## Quick Start

### Option 1: Download Release (Recommended)

1. Go to the [Releases](../../releases) section
2. Download the latest `EquipmentTracker-Windows-x64.zip`
3. Extract to your desired location
4. Run `EquipmentTracker.exe`

### Option 2: Build from Source

1. **Clone the repository**
   ```bash
   git clone https://github.com/kalantariamin1369-ux/equipment-tracker-desktop.git
   cd equipment-tracker-desktop
   ```

2. **Install Prerequisites**
   - Visual Studio 2019 or later
   - .NET Framework 4.8 SDK

3. **Install Dependencies**
   ```bash
   # Open Package Manager Console in Visual Studio
   Install-Package System.Data.SQLite
   ```

4. **Build and Run**
   - Open `EquipmentTracker.sln` in Visual Studio
   - Build the solution (Ctrl+Shift+B)
   - Run the application (F5)

## Usage Guide

### Equipment Management

#### Adding Equipment
1. Fill in the equipment details in the input fields
   - **Name**: Equipment name (required)
   - **Category**: Equipment category (optional)
   - **Quantity**: Initial quantity
   - **Min Stock**: Minimum stock alert threshold
2. Click "Add Equipment"

#### Updating Equipment
1. Select equipment from the grid
2. Modify values in the input fields
3. Click "Update Selected"
4. Or double-click cells in the grid for inline editing

#### Quantity Adjustments
1. Select equipment from the grid
2. Use "Add Qty" or "Remove Qty" buttons
3. Enter the amount to adjust
4. Changes are automatically logged

#### Deleting Equipment
1. Select equipment from the grid
2. Click "Delete Selected"
3. Confirm the deletion

### Search and Filtering

- **Search Box**: Type to filter by equipment name or category
- **Category Filter**: Select specific category from dropdown
- **Column Sorting**: Click column headers to sort
- **Low Stock Highlighting**: Items below minimum stock are highlighted in red

### Transaction History

- Switch to "Transaction History" tab
- View all equipment changes with timestamps
- See old/new quantities and change amounts
- Review notes for each transaction

### File Operations

#### CSV Export
1. Go to File → Export CSV
2. Choose save location
3. All current equipment data is exported

#### Database Backup
1. Go to File → Backup Database
2. Choose backup location
3. Complete database is backed up

## Database Schema

### Equipment Table
```sql
CREATE TABLE Equipment (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Quantity INTEGER DEFAULT 0,
    Category TEXT,
    MinStockLevel INTEGER DEFAULT 0,
    LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### Transactions Table
```sql
CREATE TABLE Transactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EquipmentId TEXT,
    EquipmentName TEXT,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    ChangeType TEXT,
    OldQuantity INTEGER,
    NewQuantity INTEGER,
    Notes TEXT,
    FOREIGN KEY(EquipmentId) REFERENCES Equipment(Id)
);
```

## Project Structure

```
EquipmentTracker/
├── EquipmentTracker.sln          # Visual Studio solution
├── README.md                     # This file
├── LICENSE                       # MIT license
├── build.bat                     # Build script
└── EquipmentTracker/
    ├── EquipmentTracker.csproj    # Project file
    ├── App.config                 # Application configuration
    ├── packages.config            # NuGet packages
    ├── Program.cs                 # Application entry point
    ├── Models.cs                  # Data models
    ├── EquipmentRepository.cs     # Database access layer
    ├── MainForm.cs                # Main user interface
    ├── MainForm.Designer.cs       # UI designer file
    ├── Settings.cs                # Application settings
    ├── Utilities.cs               # Helper functions
    └── Properties/
        └── AssemblyInfo.cs        # Assembly information
```

## Development

### Architecture

The application follows a layered architecture:

- **Presentation Layer**: Windows Forms UI (`MainForm.cs`)
- **Business Logic**: Equipment management logic
- **Data Access Layer**: SQLite repository (`EquipmentRepository.cs`)
- **Data Models**: Entity classes (`Models.cs`)
- **Utilities**: Helper functions and settings

### Key Components

- **EquipmentRepository**: Handles all database operations
- **Equipment/Transaction Models**: Data entities with property change notification
- **MainForm**: Tabbed interface with equipment grid and transaction history
- **Settings**: Theme and configuration management
- **Utilities**: Logging, CSV operations, file handling

### Building

```bash
# Command line build
msbuild EquipmentTracker.sln /p:Configuration=Release

# Or use the provided build script
build.bat
```

### Testing

The application includes comprehensive error handling and logging:

- Check `Logs/` folder for error logs
- Database operations are wrapped in try-catch blocks
- UI operations validate input data
- Graceful degradation for missing dependencies

## Contributing

Contributions are welcome! Please follow these steps:

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/new-feature
   ```
3. **Make your changes**
4. **Test thoroughly**
5. **Commit with clear messages**
   ```bash
   git commit -am 'Add new feature: description'
   ```
6. **Push to your fork**
   ```bash
   git push origin feature/new-feature
   ```
7. **Create a Pull Request**

### Coding Standards

- Follow C# naming conventions
- Use meaningful variable and method names
- Add comments for complex logic
- Include error handling for all database operations
- Write unit tests for new features

## Troubleshooting

### Common Issues

**"System.Data.SQLite" not found**
```bash
Install-Package System.Data.SQLite
```

**Database access errors**
- Ensure application has write permissions
- Check that database file isn't locked by another process

**Missing .NET Framework**
- Install .NET Framework 4.8 from Microsoft

**Build errors**
- Restore NuGet packages: `nuget restore`
- Clean and rebuild solution

**Performance issues**
- Application is optimized for 10,000+ items
- Consider implementing pagination for larger datasets

### Getting Help

1. Check the [Issues](../../issues) page for similar problems
2. Review error logs in the `Logs/` directory
3. Create a new issue with:
   - Steps to reproduce
   - Error messages
   - System information
   - Log files (if applicable)

## Roadmap

### Version 1.1 (Planned)
- [ ] Advanced reporting and analytics
- [ ] Equipment photos and attachments
- [ ] Barcode scanning integration
- [ ] Multi-user support

### Version 1.2 (Future)
- [ ] Web-based interface
- [ ] REST API for integration
- [ ] Equipment maintenance scheduling
- [ ] Location tracking
- [ ] Mobile app companion

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with C# and Windows Forms
- SQLite for database storage
- Icons from [Flaticon](https://www.flaticon.com)
- Inspired by modern inventory management needs

## Support

⭐ **Star this repository** if you find it useful!

📧 **Contact**: [Create an issue](../../issues) for support

🐛 **Bug Reports**: Use the [issue tracker](../../issues)

💡 **Feature Requests**: Open an [issue](../../issues) with your suggestion

---

**Made with ❤️ for equipment management**