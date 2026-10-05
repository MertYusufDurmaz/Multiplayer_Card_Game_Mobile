# 🃏 Multiplayer Card Game

![Unity](https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Node.js](https://img.shields.io/badge/Node.js-43853D?style=for-the-badge&logo=node.js&logoColor=white)
![Socket.io](https://img.shields.io/badge/Socket.io-010101?style=for-the-badge&logo=socket.io&logoColor=white)

**Developer:** Mert Yusuf Durmaz

## 🇬🇧 English

### 📌 About the Project
CardCase is a real-time multiplayer Trading Card Game (TCG) developed with Unity. The project demonstrates a complete full-stack game development cycle, featuring a custom Node.js backend for matchmaking and real-time combat synchronization, alongside a robust client-side architecture.

### ✨ Key Features
* **Real-Time Multiplayer:** Seamless online matchmaking and gameplay using Socket.io and Node.js.
* **Server-Client Synchronization:** Deterministic combat resolution ensuring no desync issues between clients.
* **Smart Bot AI:** Dynamic bot logic with varying difficulty levels (Easy, Medium, Hard) that evaluates board states when offline.
* **Dynamic Skill System:** Randomized active skills (Heal, Attack Boost, Shield, etc.) impacting battle outcomes.
* **Responsive UI:** Fully scalable interface utilizing Unity Canvas Scaler and Anchor systems, optimized for both desktop and mobile screens.
* **Juicy Game Feel:** Integrated DOTween for smooth card animations, hover effects, and combat impact visuals.

### 🛠️ Architecture & Clean Code Practices
* **Object Pooling:** Efficient memory management by recycling card objects during gameplay instead of continuous Instantiate/Destroy calls.
* **SOLID Principles:** Decoupled systems (e.g., `TurnManager`, `BattleManager`, `NetworkManager`) for high cohesion and low coupling.
* **Cloud Hosting:** The Node.js server is continuously hosted and maintained via **Render** cloud services.

### 🚀 How to Run
1. Ensure the Node.js server is running (either locally or via the cloud endpoint).
2. Open the Unity project and load the `GameScene`.
3. Enter a username, select a game mode (Online or Bot), and enjoy!

---

## 🇹🇷 Türkçe

### 📌 Proje Hakkında
CardCase, Unity ile geliştirilmiş gerçek zamanlı, çok oyunculu bir Kart Savaş Oyunudur (TCG). Bu proje, eşleştirme (matchmaking) ve anlık savaş senkronizasyonu için Node.js tabanlı özel bir sunucu ile güçlü bir istemci (client) mimarisini birleştiren tam kapsamlı bir oyun geliştirme döngüsünü sergilemektedir.

### ✨ Temel Özellikler
* **Gerçek Zamanlı Çok Oyunculu Mod:** Socket.io ve Node.js kullanılarak kesintisiz online eşleştirme ve oynanış.
* **Sunucu-İstemci Senkronizasyonu:** İstemciler arası (desync) hatalarını önleyen, deterministik savaş hesaplama sistemi.
* **Akıllı Bot Yapay Zekası:** Çevrimdışı durumlarda oyun tahtasını analiz edebilen, farklı zorluk seviyelerine (Kolay, Orta, Zor) sahip dinamik bot mantığı.
* **Dinamik Yetenek Sistemi:** Savaşın gidişatını değiştiren rastgele aktif yetenekler (Can Yenileme, Saldırı Artışı, Kalkan vb.).
* **Duyarlı (Responsive) Arayüz:** Unity Canvas Scaler ve Anchor sistemleri ile hem bilgisayar hem de mobil ekranlara %100 uyumlu, ölçeklenebilir arayüz.
* **Akıcı Animasyonlar:** Kart hareketleri, savaş etkileri ve arayüz geçişleri için DOTween entegrasyonu.

### 🛠️ Mimari & Temiz Kod Prensipleri
* **Object Pooling:** Oyun esnasında sürekli obje yaratıp/yok etmek yerine, kartları havuzdan çekip geri döndürerek sağlanan yüksek bellek optimizasyonu.
* **SOLID Prensipleri:** Birbirinden bağımsız çalışan, modüler yöneticiler (`TurnManager`, `BattleManager`, `NetworkManager`) sayesinde temiz kod yapısı.
* **Bulut Sunucu (Cloud):** Node.js sunucusu, **Render** bulut servisi üzerinde 7/24 kesintisiz olarak barındırılmaktadır.

### 🚀 Nasıl Çalıştırılır?
1. Node.js sunucusunun çalıştığından (yerel ağda veya bulut üzerinde) emin olun.
2. Unity projesini açın ve `GameScene` sahnesini başlatın.
3. Kullanıcı adı girin, oyun modunu (Online veya Bot) seçin ve oynamaya başlayın!
