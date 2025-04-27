# KZ Cheats

**KZ Cheats** is an open-source cheat management system for singleplayer games, built using WPF and MahApps.Metro UI.  
It consists of two applications: one for users to load cheats, and one for developers to create and manage cheat files (`.kzc`).

---

## 📦 Projects

- **KZCheats** — Main user application for loading `.kzc` cheat tables and applying them to running games.
- **KZCheatsDev** — Developer tool for creating and editing `.kzc` cheat files.
- **KZCheats.Core** — Shared library containing common models, services, and utilities.

---

## 🚀 Features

- Load `.kzc` files containing cheat configurations.
- Memory read/write operations on running game processes (requires administrator privileges).
- Support for:
  - Pointer chains (base address + offsets)
  - Value freezing (locking memory values)
  - Multiple data types: `int`, `float`, `double`, `byte`, `string`
- Modern UI powered by MahApps.Metro.
- Developer-side validation to ensure no broken or incomplete cheat files.
- Automatic version checking and updating system.
- Persistent storage of recently used `.kzc` files.
- Open-source and community-driven development.

---

## ⚙️ How It Works

1. Developers create `.kzc` cheat files using **KZCheatsDev**.
2. Users load `.kzc` files into **KZCheats**.
3. Cheats are applied to the target game's memory in real-time.

---

## ⚠️ Important

- **KZ Cheats is strictly intended for singleplayer game use only.**
- **Usage in multiplayer games is not supported and strongly discouraged.**
- Always run the application with **administrator privileges** for full functionality.

---

## 📜 License

This project is licensed under the **MIT License**.  
Feel free to use, modify, and contribute!

---

## 💬 Contributing

Pull requests are welcome!  
For major changes, please open an issue first to discuss what you would like to change.

---

## 📧 Contact

- Issues: [Open an Issue](https://github.com/Adilx05/KZCheats/issues)
- Pull Requests: [Submit a Pull Request](https://github.com/Adilx05/KZCheats/pulls)
- GitHub Repository: [KZCheats Repository](https://github.com/Adilx05/KZCheats)
