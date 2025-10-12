# Processors Architecture

This document describes the refactored processor architecture that was extracted from the monolithic RuleEngine.cs file.

## Structure

### Interfaces
- **IPostProcessor.cs** - Interface for post-processing operations
- **ITransformationProcessor.cs** - Interface for transformation operations

### Post Processors
Post processors handle content cleanup and formatting after extraction:

- **TrimWhitespaceProcessor.cs** - Removes leading/trailing whitespace
- **FormatTextProcessor.cs** - Handles text formatting (newlines, spaces)
- **CleanHtmlProcessor.cs** - Removes HTML tags from content
- **DecodeHtmlProcessor.cs** - Decodes HTML entities

### Transformation Processors
Transformation processors handle content modification and extraction:

- **FormatTransformationProcessor.cs** - Basic formatting with max length support
- **TruncateTransformationProcessor.cs** - Content truncation based on length
- **CustomTransformationProcessor.cs** - Advanced processing including:
  - Regex pattern matching and extraction
  - Format string processing
  - HTML comment removal
  - Run block removal
  - XML tag removal
  - Whitespace cleaning
  - Formatting preservation

## Benefits

1. **Maintainability** - Each processor is in its own file
2. **Extensibility** - Easy to add new processors
3. **Testability** - Each processor can be tested independently
4. **Separation of Concerns** - Clear separation between different types of processing
5. **Code Size** - RuleEngine.cs reduced from 817 to 471 lines (42% reduction)

## Usage

All processors are automatically registered in the RuleEngine constructor and accessed through their respective interfaces. The namespace `Novex.Analyzer.Processors` is imported in RuleEngine.cs for seamless integration.