# Contributing to Adventure Backpacks

Thank you for your interest in contributing to **Adventure Backpacks**! Whether you are reporting an issue, requesting new features, or submitting code, please review these guidelines before getting started.

---

## 🐛 Reporting Issues & Bugs

Before opening an issue, please search existing open and closed issues to ensure your problem has not already been reported or resolved.

### ⚠️ Mandatory Requirement: Session Logs (`LogOutput.log`)
When reporting an issue, you **MUST** provide the complete, unedited log file from BepInEx for the exact game session where the issue occurred:
* **Client Log**: Locate `BepInEx/LogOutput.log` (or within your mod manager profile directory, e.g. Gale / r2modman / Thunderstore).
* **Dedicated Server Log (If Applicable)**: If the issue occurred on or involved a dedicated server, you **MUST provide BOTH the client `LogOutput.log` AND the server `LogOutput.log`**.
* **Do Not Edit or Truncate**: Please attach the full file (drag and drop the `.log` into the GitHub issue, or link a Gist / pastebin). Snippets or single-line error messages are rarely sufficient to diagnose mod interactions or root causes.

> [!IMPORTANT]
> Issues opened without an attached `LogOutput.log` from the affected session cannot be investigated and will be placed on hold until logs are provided.

### What to Include in Your Bug Report:
1. **Clear Description**: A concise summary of what went wrong.
2. **Steps to Reproduce**: Exact steps from game launch to the bug occurring.
3. **Expected vs. Actual Behavior**: What did you expect to happen, and what actually happened?
4. **Environment**:
   - Game version
   - Mod version (e.g. `2.3.4`)
   - Game mode: Solo / Local, Player-Hosted (listen server), or Dedicated Server.
5. **Mod List**: If you are running multiple mods, provide your mod list or mod manager profile code.

---

## 💡 Feature Requests & Suggestions

We welcome ideas for enhancements and new capabilities! When submitting a feature request:
* Search open and closed feature requests first to avoid duplicates.
* Describe the problem or gameplay friction you want to solve.
* Describe your proposed solution or desired behavior.
* If you are a mod developer requesting API hooks or integration points, specify the exact event, query, or data model you need.

---

## 🔀 Submitting Pull Requests (PRs)

We welcome community contributions, but please note our strict review policies:

### ⚠️ Mandatory Policy: Open an Issue First!
> **Pull Requests are ONLY reviewed and accepted if they are linked to an existing, pre-approved open issue.**
> **Opening a Pull Request without an associated open issue will result in the PR being automatically closed without review.**

To submit code:
1. Open an Issue first describing the bug or feature, or find an existing open issue.
2. Discuss your proposed implementation with the maintainer.
3. In your Pull Request description, explicitly link the issue: `Fixes #123` or `Closes #123`.

### Coding & Engineering Standards
All pull requests must strictly adhere to the repository's engineering rules (see `AGENTS.md`):
* **Explicit Typing Over `var`**: Explicitly declare types for local variables, collections, and Unity components (e.g. `int`, `string`, `Inventory`, `Transform`). `var` is only permitted in target-typed `new()`.
* **Unity Object Lifetime Semantics**: Never use `?.`, `??`, `is null`, or `is not null` on `UnityEngine.Object` types. Use standard `obj == null`, `obj != null`, or `if (!obj)`.
* **Comments & Documentation**: Do not use `/// <summary>` XML doc blocks on internal methods (these are strictly reserved for public API surfaces). Code should be clean and self-documenting.
* **No Reflection on Internals**: Never use reflection or Harmony patches to manipulate private fields or internal classes.
* **Three Game Modes Compatibility**: Ensure changes are safe across Solo play, Player-Hosted servers, and Dedicated Servers (headless). Guard client visuals/UI from executing on dedicated servers.
* **Do NOT Bump Versions**: Do NOT edit version numbers in `manifest.json`, `AssemblyInfo.cs`, or code, and do NOT touch `CHANGELOG.md` or release archives. Release staging and version bumping are handled exclusively by maintainers during the release cycle.

---

## 💬 Community & Discussion

Have questions, want to chat with fellow Viking modders, or need help troubleshooting?
Join our community on Discord: **[Vapok Gaming Discord](https://discord.gg/5YAJkRFBXt)**.
