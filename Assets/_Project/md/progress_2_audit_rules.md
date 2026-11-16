# PROGRESS_2_AUDIT.md — Night Stand (Demo)
**Target Event:** On-site Progress Update #2 (วิชา GI262 Data Structures and Algorithms)  
**Date:** วันอังคารที่ 17 พฤศจิกายน 2569 (ห้องปฏิบัติการ A1-405)  
**Objective:** ให้ AI (Antigravity) ตรวจสอบความพร้อมของโปรเจกต์ Unity ทั้งหมด ให้ผ่านเกณฑ์ Playable Build, Data Structures Demonstration และ Class Diagram Alignment ก่อนนำเสนอในชั้นเรียน

---

## 1. ภารกิจการตรวจสอบและปรับปรุงโปรเจกต์ (AI Audit Checklist)
เมื่อได้รับคำสั่งให้อ่านไฟล์นี้ ให้ Antigravity ตรวจสอบสคริปต์และฉากทั้งหมดในโปรเจกต์ (`Assets/_Project`) ตามหัวข้อต่อไปนี้:

### A. ตรวจสอบ Playable Build & Core Gameplay Loop (เกณฑ์บังคับสูงสุด)
ต้องมั่นใจว่าโปรเจกต์ไม่มี Compile Error และสามารถกด Play หรือ Build `.exe` เล่นได้ลื่นไหลตั้งแต่ต้นจนจบกะ (18:00 – 06:00 น.) โดยไม่ค้าง:
- [ ] **No Compile Errors / Missing References:** ตรวจสอบทุก GameObject ใน `GreyboxTestScene` และ Prefab ว่าไม่มีช่อง Inspector ว่าง (`None / Missing`) ที่ทำให้เกิด `NullReferenceException`
- [ ] **Complete Gameplay Loop:**
  1. รถเคลื่อนที่เข้ามาจอดหน้าไม้กั้นได้จริง (`VehicleSpawner` -> `VehicleController`)
  2. ระบบ `Waypoint` แสดงกรอบจุดตรวจตาม Wave ได้ถูกต้อง (Wave 1 โชว์จุดพื้นฐานครบ / Wave 2-3 โชว์เฉพาะจุดใหม่)
  3. ผู้เล่นเดินตรวจ (`WASD`), เปิด/ปิดไฟฉาย (`Flashlight`), หยิบดูเอกสาร (`InformationPaper`), และคุยซักถาม NPC (`DialogueSystem`) ได้จริง
  4. ปุ่มตัดสินใจทำงานครบ 3 ทางเลือก: **Allow** (เปิดไม้กั้นรถผ่าน), **Deny** (ไล่รถกลับ), และ **Eliminate** (ใช้อุปกรณ์ปราบ -> เล่น Effect -> ตัดจอดำพร้อมเสียง -> รถหายไป)
  5. เพื่อน AI (`AIPartner`) มอบหมาย Quest และส่งไอเทมรางวัลมาวางที่จุด `ItemSpawner` ได้จริง
  6. ฉากจบทั้ง 3 แบบทำงานถูกต้อง: **Instant Arrest** (กำจัดคนปกติ), **Bad Ending** (06:00 น. มี Anomaly หลุดเข้าหมู่บ้าน), และ **Good Ending** (06:00 น. คัดกรองสำเร็จ)
- [ ] **Thai Font Support (TextMeshPro):** ตรวจสอบว่า UI ทั้งหมดใช้ฟอนต์ `Sarabun-Regular SDF` และแสดงข้อความภาษาไทยได้สมบูรณ์ (ไม่มีกล่องสี่เหลี่ยม `□□□`)

### B. ตรวจสอบโครงสร้างข้อมูลและอัลกอริทึม (Data Structures & Big-O Audit)
วิชา GI262 บังคับให้ชี้แจงจุดที่นำ Data Structure มาใช้งานจริงในเกม ให้ตรวจสอบและ Refactor โค้ด (หากจำเป็น) ให้มีโครงสร้างทั้ง 4 ชนิดชัดเจน พร้อมใส่ Comment ระบุ **ชื่อโครงสร้างข้อมูล** และ **Time Complexity (Big-O)** เหนือตัวแปร/เมธอดหลักเพื่อใช้แคปหน้าจอลงสไลด์:
1. **`Queue<T>` (FIFO):** ใช้ในระบบคิวรถยนต์เข้าป้อมยาม (`Enqueue` เมื่อสุ่มรถใหม่, `Dequeue` เมื่อเรียกเข้าจุดตรวจ) -> *Time Complexity: $O(1)$*
2. **`Dictionary<TKey, TValue>` / `HashSet<T>`:** ใช้ในระบบฐานข้อมูลทะเบียนลูกบ้าน (`ResidentDatabase`) และการเช็กประวัติรถที่เข้าหมู่บ้านไปแล้วเพื่อจับผิดตัวปลอม (**Doppelgänger**) -> *Time Complexity: Search/Insert $O(1)$*
3. **`Tree / Graph Structure`:** ใช้ใน `DialogueSystem` (`DialogueNode`) สำหรับเก็บโครงสร้างบทสนทนาซักถาม NPC แบบแตกแขนง -> *Time Complexity: Traversal $O(h)$ หรือ $O(1)$ ต่อโหนด*
4. **`List<T>` / `Array`:** ใช้ในระบบ `Waypoint` (วนลูปเปิด/ปิดจุดตรวจตาม Wave), `Equipment` (ช่องอุปกรณ์ 1–3), และ `QuestManager` -> *Time Complexity: Access $O(1)$, Iteration $O(n)$*

### C. ตรวจสอบความสอดคล้องกับ Updated Class Diagram
ตรวจสอบชื่อคลาส ตัวแปร และความสัมพันธ์ในโค้ดให้ตรงกับ Class Diagram ฉบับอัปเดต:
- [ ] `Vehicle` มีการบรรจุ `NPC` (`occupants`) อยู่ภายใน (`Aggregation`) และแยก `frontPlate` / `rearPlate`
- [ ] `NormalPerson` และ `Anomaly` สืบทอดจาก `NPC` (`Inheritance`) โดย `NormalPerson` มี `isDocumentValid` และ `Anomaly` มี `Eliminate()`
- [ ] `EliminatorTool`, `InspectionTool`, `Key`, `InformationPaper` สืบทอดจาก `Item` (`Inheritance`)
- [ ] `AIPartner` เชื่อมต่อกับ `QuestManager` และ `ItemSpawner`

---

## 2. สิ่งที่ AI ต้องสรุปส่งมอบกลับมาให้ทีม (Deliverables for Presentation)
หลังจากตรวจสอบและแก้ไขโค้ดเสร็จแล้ว ให้ Antigravity สรุปข้อมูล 3 ส่วนนี้กลับมาในแชต เพื่อนำไปวางในสไลด์พรีเซนต์และตารางส่งงานกลางทันที:

1. **ตารางสรุป Data Structures & Algorithms (สำหรับสไลด์ที่ 4):**
   - แสดงคอลัมน์: `Data Structure` | `Script File & Line` | `บทบาทในเกม Night Stand` | `Time Complexity (Big-O)`
2. **รายการจุดแนะนำสำหรับแคปหน้าจอโค้ด (Screenshot Guide):**
   - ระบุชื่อไฟล์และช่วงบรรทัดที่สวยและชัดเจนที่สุด 3–4 จุด เพื่อแคปไปวางคู่กับภาพในเกม
3. **สรุปปัญหาทางเทคนิคที่พบและวิธีแก้ไข (Issues & Solutions สำหรับสไลด์ที่ 6) + แผนงานรอบถัดไป (Next Sprint Plan สำหรับสไลด์ที่ 7):**
   - เขียนสรุปสั้นๆ กระชับ เป็นทางการ พร้อมก๊อปปี้ลงสไลด์

---

## 3. สิ่งที่ทีมต้องเตรียมนอกเหนือจากโค้ด (Manual Checklist ก่อนเข้าห้อง A1-405)
- [ ] **Build ตัวเกมสำรอง:** กด `File > Build Profiles > Build` สร้างไฟล์ `.exe` เก็บไว้ในเครื่อง, Flash Drive และ Google Drive
- [ ] **อัดคลิปวิดีโอสาธิต (1–3 นาที):** เล่นโชว์ตั้งแต่เริ่มกะ, ตรวจรถ, คุย NPC, ใช้ไฟฉาย/อุปกรณ์ปราบ Anomaly (โชว์จังหวะตัดจอดำ), จนถึงหน้าจอ Ending แล้วอัปโหลดขึ้น YouTube (Unlisted) หรือ Google Drive
- [ ] **อัปเดตตารางกลาง Google Sheets:** แนบลิงก์สไลด์ (คอลัมน์ *"Slide พรีเซ้นครั้งที่ 2"*), ลิงก์คลิปวิดีโอ และลิงก์ GitHub/Google Drive Source Code ให้เรียบร้อยก่อนเวลาเรียน