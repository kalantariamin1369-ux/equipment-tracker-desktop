# Changelog

All notable changes to the Equipment Tracker Desktop application will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned Features
- Advanced reporting and analytics dashboard
- Equipment photos and file attachments
- Barcode scanning integration
- Multi-user support with authentication
- Equipment maintenance scheduling
- Location tracking for equipment
- Mobile app companion
- Web-based interface option

## [1.0.0] - 2025-10-26

### Added
- Initial release of Equipment Tracker Desktop
- Core equipment inventory management (CRUD operations)
- SQLite database with automatic table creation
- Transaction history logging for all changes
- Search and filtering by equipment name and category
- Low stock alerts with visual highlighting
- CSV export functionality for equipment data
- Database backup and restore capability
- Professional tabbed user interface
- Comprehensive error handling and logging
- Light theme support (Dark theme framework ready)
- Settings persistence using JSON
- Windows Forms application targeting .NET Framework 4.8

### Features
- **Equipment Management**
  - Add new equipment with name, category, quantity, and minimum stock level
  - Update existing equipment details
  - Delete equipment with confirmation
  - Inline quantity adjustments (add/remove)
  - Sortable data grid with column sorting
  
- **Search and Filtering**
  - Real-time search by equipment name or category
  - Category dropdown filter
  - Case-insensitive text search
  
- **Transaction Tracking**
  - Complete audit trail of all equipment changes
  - Timestamps for all transactions
  - Change type categorization (Create, Update, Delete, Add, Remove)
  - Notes and reason tracking
  
- **Data Management**
  - CSV export with customizable column mapping
  - Database backup to specified location
  - Automatic database initialization
  - Error logging to daily log files
  
- **User Interface**
  - Tabbed interface (Equipment and Transaction History)
  - Status bar with equipment statistics
  - Menu system with file operations
  - Responsive layout with docked panels
  - Low stock visual alerts (red highlighting)
  
- **System Integration**
  - Windows desktop application
  - Portable executable (no installation required)
  - Settings stored in user AppData folder
  - Local SQLite database storage

### Technical Details
- Built with C# and Windows Forms
- Targets .NET Framework 4.8
- SQLite database for data persistence
- System.Text.Json for settings serialization
- Comprehensive exception handling
- Professional logging system
- Object-oriented architecture with separation of concerns

### Dependencies
- System.Data.SQLite (1.0.118.0)
- Microsoft.VisualBasic (for InputBox functionality)
- .NET Framework 4.8

### System Requirements
- Windows 10 or later
- .NET Framework 4.8 (usually pre-installed)
- ~50MB disk space for application
- Write permissions for database and log files

---

## Version Numbering

This project uses semantic versioning (MAJOR.MINOR.PATCH):
- **MAJOR**: Incompatible API changes or major feature overhauls
- **MINOR**: New functionality added in a backwards compatible manner
- **PATCH**: Backwards compatible bug fixes and minor improvements

## Release Types

- **🎉 Major Release**: New major features, significant UI changes, or breaking changes
- **✨ Minor Release**: New features, enhancements, or improvements
- **🐛 Patch Release**: Bug fixes, performance improvements, or minor updates
- **🔧 Hotfix**: Critical bug fixes or security patches

## Feedback and Contributions

We welcome feedback, bug reports, and feature requests through the GitHub issues system.
Please see our contributing guidelines for information on how to contribute to the project.