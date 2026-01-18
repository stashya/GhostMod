# Changelog

All notable changes to GhostMod will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-18

### Added
- Initial release
- Personal ghost recording and racing
- Shared ghost support (race against other players' ghosts)
- Live delta timer showing time ahead/behind
- Support for all 10 routes:
  - Akina Downhill & Uphill
  - Akagi Downhill & Uphill
  - Irohazaka Downhill & Uphill
  - Usui Downhill & Uphill
  - Myogi Downhill & Uphill
- Visual ghost car with:
  - Semi-transparent appearance
  - Working brake lights
  - Working headlights
  - Animated wheel steering
  - Wheel spin animation
- In-game menu (press G)
- Ghost visibility toggle (press H during race)
- Automatic folder creation on first launch
- Security validation for shared ghost files
- WarmTofuMod compatibility
- Standalone operation without dependencies

### Security
- File size limits on shared ghosts (50MB max)
- Frame count limits (100,000 max)
- Path traversal protection
- String length validation
- Float value sanitization
- Player name sanitization

---

## Future Plans

- [ ] Leaderboard integration
- [ ] Ghost export with metadata
- [ ] Multiple ghosts on screen
- [ ] Ghost replay viewer
- [ ] Split time checkpoints
