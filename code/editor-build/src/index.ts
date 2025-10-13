import {
  autocompletion,
  closeBrackets,
  closeBracketsKeymap,
  completionKeymap,
} from '@codemirror/autocomplete';
import {
  defaultKeymap,
  history,
  historyKeymap,
} from '@codemirror/commands';
import { markdown } from '@codemirror/lang-markdown';
import { yaml } from '@codemirror/lang-yaml';
import {
  bracketMatching,
  defaultHighlightStyle,
  foldGutter,
  foldKeymap,
  indentOnInput,
  indentUnit,
  syntaxHighlighting,
} from '@codemirror/language';
import {
  highlightSelectionMatches as searchHighlightSelectionMatches,
  searchKeymap,
} from '@codemirror/search';
import {
  EditorState,
  Extension,
} from '@codemirror/state';
import { oneDark } from '@codemirror/theme-one-dark';
import {
  crosshairCursor,
  drawSelection,
  dropCursor,
  EditorView,
  highlightActiveLine,
  highlightActiveLineGutter,
  highlightSpecialChars,
  keymap,
  lineNumbers,
  rectangularSelection,
} from '@codemirror/view';

import {
  markdownDarkHighlighting,
  markdownDarkTheme,
  markdownLightHighlighting,
  markdownLightTheme,
} from './themes';

// Define the editor options interface
interface EditorOptions {
  theme?: "light" | "dark";
  language?: "markdown" | "yaml" | "text";
  lineNumbers?: boolean;
  foldGutter?: boolean;
  dropCursor?: boolean;
  allowMultipleSelections?: boolean;
  indentOnInput?: boolean;
  bracketMatching?: boolean;
  closeBrackets?: boolean;
  searchKeymap?: boolean;
  tabSize?: number;
  height?: string;
  minHeight?: string;
  maxHeight?: string;
}

// Define the editor info interface
interface EditorInfo {
  lineCount: number;
  length: number;
  selection: {
    from: number;
    to: number;
    empty: boolean;
  };
}

// Define the editor result interface
interface EditorResult {
  elementId: string;
}

// Main CodeMirror setup class
class CodeMirrorSetup {
  private editors: Map<string, EditorView> = new Map();

  /**
   * Creates a new CodeMirror editor instance
   */
  createEditor(
    elementId: string,
    initialValue: string = "",
    options: EditorOptions = {}
  ): EditorResult | null {
    const element = document.getElementById(elementId);
    if (!element) {
      console.error("Element not found:", elementId);
      return null;
    }

    // Default options
    const defaultOptions: EditorOptions = {
      theme: "light",
      language: "markdown",
      lineNumbers: true,
      foldGutter: true,
      dropCursor: false,
      allowMultipleSelections: false,
      indentOnInput: true,
      bracketMatching: true,
      closeBrackets: true,
      searchKeymap: true,
      tabSize: 2,
      height: "400px",
      minHeight: "200px",
    };

    const config = { ...defaultOptions, ...options };

    // Build extensions array
    const extensions: Extension[] = [
      // Basic setup
      highlightSpecialChars(),
      history(),
      drawSelection(),
      dropCursor(),
      EditorState.allowMultipleSelections.of(
        config.allowMultipleSelections || false
      ),
      indentOnInput(),
      bracketMatching(),
      closeBrackets(),
      autocompletion(),
      rectangularSelection(),
      crosshairCursor(),
      highlightActiveLine(),
      highlightActiveLineGutter(),
      EditorView.lineWrapping, // 自动换行

      // Keymaps
      keymap.of([
        ...closeBracketsKeymap,
        ...defaultKeymap,
        ...searchKeymap,
        ...historyKeymap,
        ...foldKeymap,
        ...completionKeymap,
      ]),

      // Indentation
      indentUnit.of(" ".repeat(config.tabSize || 2)),

      // Update listener for Blazor integration
      EditorView.updateListener.of((update) => {
        if (update.docChanged) {
          this.dispatchChangeEvent(elementId, update.state.doc.toString());
        }
      }),

      // Basic editor styling (will be overridden by theme)
      EditorView.theme({
        "&": {
          height: config.height || "400px",
          minHeight: config.minHeight || "200px",
          maxHeight: config.maxHeight || null,
        },
      }),
    ];

    // Add line numbers if enabled
    if (config.lineNumbers) {
      extensions.push(lineNumbers());
    }

    // Add fold gutter if enabled
    if (config.foldGutter) {
      extensions.push(foldGutter());
    }

    // Add language support and theme
    if (config.language === "markdown") {
      extensions.push(markdown());

      // Add theme-specific styling and syntax highlighting
      if (config.theme === "dark") {
        extensions.push(markdownDarkTheme);
        extensions.push(syntaxHighlighting(markdownDarkHighlighting));
      } else {
        extensions.push(markdownLightTheme);
        extensions.push(syntaxHighlighting(markdownLightHighlighting));
      }
    } else if (config.language === "yaml") {
      extensions.push(yaml());
      if (config.theme === "dark") {
        extensions.push(oneDark);
      }
      extensions.push(
        syntaxHighlighting(defaultHighlightStyle, { fallback: true })
      );
    } else {
      // For non-markdown, use default themes
      if (config.theme === "dark") {
        extensions.push(oneDark);
      }
      extensions.push(
        syntaxHighlighting(defaultHighlightStyle, { fallback: true })
      );
    }

    // Add search highlighting
    extensions.push(searchHighlightSelectionMatches());

    // Create editor state
    const startState = EditorState.create({
      doc: initialValue,
      extensions: extensions,
    });

    // Clear element and create editor view
    element.innerHTML = "";
    const view = new EditorView({
      state: startState,
      parent: element,
    });

    // Store reference
    this.editors.set(elementId, view);

    console.log(
      `CodeMirror editor created for ${elementId} with ${config.theme} theme`
    );
    return { elementId: elementId };
  }

  /**
   * Dispatches change event for Blazor integration
   */
  private dispatchChangeEvent(elementId: string, value: string): void {
    const element = document.getElementById(elementId);
    if (element) {
      const customEvent = new CustomEvent("codemirror-change", {
        detail: {
          elementId: elementId,
          value: value,
        },
      });
      element.dispatchEvent(customEvent);
    }
  }

  /**
   * Sets the value of an editor
   */
  setValue(elementId: string, value: string): void {
    const view = this.editors.get(elementId);
    if (view) {
      const transaction = view.state.update({
        changes: {
          from: 0,
          to: view.state.doc.length,
          insert: value || "",
        },
      });
      view.dispatch(transaction);
    }
  }

  /**
   * Gets the value of an editor
   */
  getValue(elementId: string): string {
    const view = this.editors.get(elementId);
    return view ? view.state.doc.toString() : "";
  }

  /**
   * Focuses an editor
   */
  focus(elementId: string): void {
    const view = this.editors.get(elementId);
    if (view) {
      view.focus();
    }
  }

  /**
   * Destroys an editor instance
   */
  destroy(elementId: string): void {
    const view = this.editors.get(elementId);
    if (view) {
      view.destroy();
      this.editors.delete(elementId);
      console.log(`CodeMirror editor destroyed for ${elementId}`);
    }
  }

  /**
   * Gets editor information
   */
  getEditorInfo(elementId: string): EditorInfo | null {
    const view = this.editors.get(elementId);
    if (view) {
      const selection = view.state.selection.main;
      return {
        lineCount: view.state.doc.lines,
        length: view.state.doc.length,
        selection: {
          from: selection.from,
          to: selection.to,
          empty: selection.empty,
        },
      };
    }
    return null;
  }

  /**
   * Sets the theme of an editor
   */
  setTheme(elementId: string, theme: "light" | "dark"): void {
    const view = this.editors.get(elementId);
    if (view) {
      // Note: Theme switching requires reconfiguring extensions
      // For now, we'll just log the request
      console.log(`Theme change requested for ${elementId}: ${theme}`);
      // This would require a more complex reconfigure implementation
    }
  }

  /**
   * Inserts text at the current cursor position
   */
  insertText(elementId: string, text: string): void {
    const view = this.editors.get(elementId);
    if (view) {
      const selection = view.state.selection.main;
      view.dispatch({
        changes: {
          from: selection.from,
          to: selection.to,
          insert: text,
        },
        selection: { anchor: selection.from + text.length },
      });
    }
  }

  /**
   * Gets the current selection text
   */
  getSelection(elementId: string): string {
    const view = this.editors.get(elementId);
    if (view) {
      const selection = view.state.selection.main;
      return view.state.doc.sliceString(selection.from, selection.to);
    }
    return "";
  }
}

// Create and expose global instance
declare global {
  interface Window {
    CodeMirrorSetup: CodeMirrorSetup;
  }
}

window.CodeMirrorSetup = new CodeMirrorSetup();

// Export for potential module usage
export default CodeMirrorSetup;

// Also create a simpler global function for easier usage
(window as any).createEditor = function (
  elementId: string,
  initialValue: string = "",
  options: EditorOptions = {}
) {
  return window.CodeMirrorSetup.createEditor(elementId, initialValue, options);
};
