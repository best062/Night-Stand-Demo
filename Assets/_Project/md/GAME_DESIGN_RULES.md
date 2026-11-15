# GAME_DESIGN_RULES.md — Night Stand (Demo)

## 1. Project Overview & Tech Stack
- **Game Title:** Night Stand (Demo)
- **Genre:** First-Person Anomaly Detection / Simulation (Balanced: Detective Investigation + Light Horror/Jumpscare)
- **Target Playtime (Demo):** 5–6 Minutes per shift (In-game time: 18:00 – 06:00).
- **Engine & Pipeline:** Unity `6000.6.6f1` (Unity 6) / Universal Render Pipeline (URP) / C#.
- **Current Project State:** Clean project (No existing scripts yet). AI is authorized to generate full core architecture from scratch.
- **Core Premise:** ผู้เล่นรับบทเป็น รปภ. กะดึกประจำป้อมหน้าหมู่บ้าน มีหน้าที่ตรวจสอบรถยนต์และบุคคล (NPC) ที่ขอผ่านเข้าหมู่บ้าน โดยมี **เพื่อนร่วมงาน AI (AI Partner)** คอยติดต่อสื่อสาร มอบหมายภารกิจ (Quest) และส่งอุปกรณ์ (Equipment) ให้จากอีกฝั่งหนึ่ง

---

## 2. Scoring & Feature Architecture (Total 20 Points)
โค้ดและระบบทั้งหมดต้องครอบคลุมและแยกโครงสร้างชัดเจนตามเกณฑ์ 4 หมวดหลัก (20 คะแนน) ดังนี้:

1. **Scene (7 Points)**
   - `Waypoint / Navigate` (3p): ระบบจุดนำสายตา (3D World Marker / UI Indicator) รอบตัวรถ
     - **Dynamic Behavior:** ในช่วงแรก (คันแรกๆ / Wave 1) จะแสดงกรอบนำทางครบทุกจุดพื้นฐาน (ป้ายทะเบียน, คนขับ, เอกสาร) แต่เมื่อเข้า Wave ถัดไป กรอบพื้นฐานจะหายไป (ผู้เล่นต้องจำเอง) และจะแสดงเฉพาะ **จุดตรวจใหม่ที่ปลดล็อกตามช่วงเวลา** เท่านั้น (เช่น เบาะหลัง, ท้ายรถ)
   - `Object Spawn` (2p): ระบบสุ่มและสร้างคิวรถยนต์ (`Vehicle`) ที่ขับเข้ามาจอดรอหน้าไม้กั้นตามช่วงเวลา
   - `Item Spawn` (2p): จุดเกิดของเอกสารหลักฐานบนโต๊ะป้อมยาม (เช่น ใบรายชื่อลูกบ้าน, เอกสารทะเบียน) และอุปกรณ์ที่เพื่อน AI ส่งมาให้
2. **Character (10 Points)**
   - `Equipment` (2p): ระบบ Inventory ถือของมือขวาได้ **ทีละ 1 ชิ้น** สลับใช้งานผ่านปุ่มเลข `1-2-3` (หรือ Scroll Wheel / ช่อง Inventory) เช่น `Flashlight` (ไฟฉาย), `Key` (กุญแจ), `Eliminator` (อุปกรณ์ปราบ Anomaly)
   - `Dialogue with NPC` (3p): ระบบบทสนทนาโต้ตอบกับคนขับ/ผู้โดยสารในรถเพื่อซักถามข้อมูลและจับพิรุธ
   - `Quest` (5p): ระบบภารกิจที่สั่งการโดย **เพื่อน AI (AI Partner)** (ผ่านวิทยุ/หน้าจอติดต่อ) แบ่งเป็น Main Quest (อยู่ให้ครบกะ 06:00 น.) และ Side Quest ระหว่างคืน (เช่น ตรวจรถต้องสงสัย, ใช้กุญแจไขหาแฟ้มข้อมูลในป้อม, ปลดล็อกอุปกรณ์ใหม่)
3. **Enemy (5 Points)**
   - `Wave` (2p): แบ่งช่วงเวลา 1 กะ (5–6 นาทีจริง) ออกเป็น 3 Waves ตามเวลาในเกม:
     - **Wave 1 (18:00 – 22:00):** ระดับง่าย เน้นจับผิดเอกสารและสภาพภายนอกรถ
     - **Wave 2 (22:00 – 02:00):** ระดับปานกลาง ต้องใช้ไฟฉายส่องจุดอับ ถามจี้จุดใน Dialogue และเริ่มมีจังหวะ Jumpscare เบาๆ
     - **Wave 3 (02:00 – 06:00):** ระดับยาก ความมืดจัด บรรยากาศกดดัน Anomaly มีความซับซ้อนสูง
   - `Type` (3p): จำแนกประเภท Entity ที่เข้าป้อมเป็น 3 กลุ่มหลัก:
     1. `Normal (Valid)`: คนปกติ เอกสารถูกต้อง -> **ต้องเปิดไม้กั้นให้ผ่าน (Allow)**
     2. `Normal (Invalid / Rare Case)`: คนปกติ แต่เอกสารมีปัญหา (เช่น บัตรหมดอายุ, ลืมต่อทะเบียน) **โอกาสออกน้อยมากและตรวจจับได้ค่อนข้างยาก** -> **ต้องกดปฏิเสธ/ไล่กลับ (Deny)** *(ห้ามใช้อุปกรณ์ปราบเด็ดขาด)*
     3. `Anomaly`: สิ่งผิดปกติ/ตัวปลอม -> **ต้องใช้อุปกรณ์ปราบกำจัด (Eliminate)**
4. **Item (1 Point)**
   - `Type` (1p): แยกประเภทไอเทมและพฤติกรรมการใช้งานชัดเจน (`Document`, `Flashlight`, `Key`, `Eliminator`)

### Class Diagram Requirement
- คลาส `Vehicle` ต้องบรรจุหรืออ้างอิงคลาส `NPC` (คนขับ/ผู้โดยสาร) อยู่ภายในเสมอ เพื่อผูกข้อมูลและสถานะระหว่างตัวรถกับคนในรถเข้าด้วยกัน

---

## 3. Controls, Mechanics & Win/Loss Rules

### Controls & Interaction
- **Movement:** `WASD` เดินในป้อมยามและเดินออกไปตรวจรอบตัวรถได้ / `Mouse Look` หันมุมมอง
- **Interact (`E` หรือ `Left Click`):** กดปุ่มเปิด/ปิดไม้กั้น, คุยกับ NPC, หยิบเอกสารขึ้นมาดูแบบ 2D Overlay หรือก้มมองบนโต๊ะ (3D Diegetic), ใช้กุญแจไขตู้/ท้ายรถ
- **Equipment Use (`Left Click` ขณะถืออุปกรณ์):**
  - `Flashlight`: เปิด/ปิดไฟฉายส่องจุดอับในรถ
  - `Eliminator` (อุปกรณ์ปราบ Anomaly): เล็ง Crosshair ไปที่ตัวรถแล้วคลิกซ้าย -> เล่น Visual/Sound Effect เล็กน้อย -> **ตัดเข้าฉากดำ (Fade to Black) พร้อมเสียงกำจัด Anomaly** -> ตัดภาพกลับมา รถคันนั้นหายไปจากฉากและดำเนินเกมต่อ

### Consequences & Game Endings (Strict Rules)
1. **กำจัดคนปกติ (Eliminate Normal NPC — ทั้งแบบ Valid และ Invalid):**
   - **ผลลัพธ์:** **Game Over ทันที!** ตัดเข้าฉากจบพิเศษที่ **องค์กรรัฐบาลบุกเข้ามาจับกุมผู้เล่น** ฐานสังหารประชาชนผู้บริสุทธิ์
2. **ปล่อย Anomaly เข้าหมู่บ้าน (Allow Anomaly):**
   - **ผลลัพธ์:** เกมจะดำเนินต่อไปตามปกติ **ไม่มีอะไรเกิดขึ้นระหว่างกะ** เพื่อสร้างความระแวง แต่จะไป **เฉลยตอนจบกะ (06:00 น.)** ว่าหมู่บ้านถูกแทรกซึมและผู้เล่นแพ้ (Bad Ending)
3. **ไล่คนปกติที่เอกสารถูกต้องกลับ (Deny Valid Normal) หรือ ปล่อยคนเอกสารผิดเข้า (Allow Invalid Normal):**
   - **ผลลัพธ์:** บันทึกเป็นความผิดพลาด (Mistake Count) ส่งผลต่อเกรด/ผลการประเมินตอนจบกะ (06:00 น.) แต่ไม่ทำให้ Game Over ทันที
4. **ผ่านกะ 06:00 น. โดยไม่ปล่อย Anomaly เข้าหมู่บ้านเลย:**
   - **ผลลัพธ์:** **Win (Shift Complete)**

---

## 4. Anomaly & Case Database (Demo Scope)
*หมายเหตุ: ใน Demo เน้นใช้ Texture, แสงไฟ, เสียง, เอกสาร และ UI/Dialogue เป็นหลัก เพื่อลดภาระการปั้น Animation ที่ซับซ้อน*

- **Normal (Invalid - Rare):**
  - *บัตรผ่านหมดอายุ / สะกดชื่อหมู่บ้านผิดเล็กน้อย:* รถและคนปกติทุกอย่าง ไม่มีความหลอน ต้องสังเกตตัวเลขวันที่ในเอกสารหรือตัวสะกดอย่างละเอียดแล้วกด **Deny**
- **Wave 1 Anomalies (18:00 – 22:00):**
  - *Fake Plate:* ป้ายทะเบียนหน้า-หลังไม่ตรงกัน หรือไม่มีใน `ResidentList`
  - *Hit & Run:* กันชนหน้ามีรอยบุบ/คราบเลือด (ตรวจที่ Waypoint หน้ารถ) แต่ NPC อ้างว่าเพิ่งกลับจากซื้อของปกติ
- **Wave 2 Anomalies (22:00 – 02:00):**
  - *Stowaway:* ใช้ `Flashlight` ส่องเบาะหลังจะเห็นเงาคนซ่อนตัว (Jumpscare เสียงตกใจเบาๆ) แต่คนขับตอบว่า "มาคนเดียว"
  - *Trunk Knocking:* เดินไปท้ายรถจะได้ยินเสียงเคาะ (ใช้ `Key` เปิดเช็กได้) NPC อ้างว่าของท้ายรถกลิ้ง
  - *Doppelgänger:* ข้อมูลตรงกับลูกบ้านที่เพิ่งขับผ่านเข้าหมู่บ้านไปแล้วก่อนหน้านี้ พูดจาเหมือนท่องจำ
- **Wave 3 Anomalies (02:00 – 06:00):**
  - *Ghost Driver:* ส่องไฟฉายเข้าไปเบาะคนขับว่างเปล่า แต่มีเสียงตอบโต้ดังจากวิทยุในรถ
  - *Mimic:* ตัวรถมีคราบเมือก/โคลนประหลาด ถามจี้จุด 2-3 ครั้งเสียงจะเปลี่ยนเป็นเสียงแตกพร่า (Jumpscare)
  - *The Cult / Intruder:* รถตู้ทึบไม่มีป้ายทะเบียน ดับไฟหน้าตอนเข้าใกล้ ไม่ตอบคำถาม (พูดแค่ *"เปิดประตู"*)

---

## 5. AI Coding & Architecture Standards (Unity 6 URP)
1. **Modular & Scalable:** เขียนโค้ดแยกความรับผิดชอบชัดเจน (SOLID) เพื่อรองรับการขยายจาก Demo เป็นเกมเต็มในอนาคต
2. **Data-Driven Design:** ใช้ `ScriptableObject` สำหรับสร้างข้อมูล `VehicleDataSO`, `NPCDataSO`, `AnomalyDataSO`, `QuestDataSO`, และ `ItemDataSO` เพื่อให้ทีมสามารถเพิ่มเคสใหม่ๆ ใน Inspector ได้โดยไม่ต้องแก้โค้ด
3. **Unity 6 Input & URP:** ใช้ Unity New Input System (หรือมาตรฐาน Input ที่ตั้งค่าใน Unity 6) และจัดการแสง/Post-Processing (เช่น จังหวะตัดจอดำหรือ Jumpscare) ให้รองรับ URP
4. **Clear Setup Instructions:** ทุกครั้งที่สร้างหรือแก้ไขสคริปต์ C# ให้สรุปขั้นตอนการสร้าง GameObject, การลากใส่ Component และการผูก Reference ใน Unity Inspector แบบกระชับท้ายคำตอบเสมอ

## 6. UI & TextMeshPro Localization Constraint (IMPORTANT)
- **Language Policy (English Only for In-Game UI):** โปรเจกต์นี้ติดตั้งเฉพาะ **TextMeshPro (TMP) Default Font Asset** เท่านั้น และ **ยังไม่มีฟอนต์ภาษาไทย (No Thai Font Asset)**
- **Strict Rule for AI:** ข้อความทั้งหมดที่แสดงผลในเกมผ่าน `TMP_Text` หรือ UI (เช่น บทสนทนา NPC, รายละเอียดในเอกสาร/บัตรผ่าน, ชื่อ Quest, ข้อความวิทยุจากเพื่อน AI, ปุ่มกด และหน้าจอ Ending) **ต้องเขียนเป็นภาษาอังกฤษ (English) เท่านั้น** ห้ามใส่ข้อความภาษาไทยลงใน C# String, ScriptableObject หรือ UI Text เด็ดขาด เพื่อป้องกันปัญหาฟอนต์ไม่รองรับและแสดงผลเป็นสี่เหลี่ยม (`□□□`)