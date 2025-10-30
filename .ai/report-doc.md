## Reports and Docs Generation Rules

### AI Reports
- Unless explicitly specify `[no report]`, after each AI conversation, generate a summary report in folder `reports`, so that I can review them.
- You should respect the reports as your memories and contexts.
- All reports should be indexed with a numeral prefix to keep them in order, based on their creation time.
- If you ever found a report has no index number, add one as a fix. Such operations should **not** be written to any report.

### Docs
- For formal documentations, such as requirement analysis, technical specifications, architecture design, etc., they should be generated in folder `docs` instead of `reports`.
- Keep the docs easy to read while professional.

### Language Rules
- All reports and docs should be written in **Chinese** (中文) with necessary English terms.
- Follow the style guide in `@/.ai/chat.md`: provide Chinese translation followed by the original English term in parentheses.
- Examples: 处理器 (processor), 规则引擎 (rule engine), 字段 (field), 错误处理策略 (error handling strategy)
- Use Chinese for all explanations, descriptions, and technical content, but keep English terms for proper nouns, framework names, and technical identifiers.