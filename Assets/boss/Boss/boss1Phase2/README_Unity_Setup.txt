====================================================
BLUE GREATSWORD KNIGHT - UNITY 2D SPRITE PACK
====================================================

1. รายการไฟล์ในแพ็กเกจ (Sprites/ - พื้นหลังโปร่งใส PNG พร้อมใช้งาน)
- BlueKnight_Sheet1_Idle_Hurt_Death_Walk.png
  * แถวที่ 1: Idle (5 เฟรม)
  * แถวที่ 2: Hurt (2 เฟรม)
  * แถวที่ 3: Death (5 เฟรม)
  * แถวที่ 4: Walk (5 เฟรม)

- BlueKnight_Sheet2_Atk1_Run_Atk2_Special.png
  * แถวที่ 1: Attack 1 (5 เฟรม)
  * แถวที่ 2: Run (5 เฟรม)
  * แถวที่ 3: Attack 2 (5 เฟรม)
  * แถวที่ 4: Special (5 เฟรม)

- BlueKnight_Sheet3_Atk3_Atk4_Jump.png
  * แถวที่ 1: Attack 3 (5 เฟรม)
  * แถวที่ 2: Attack 4 (5 เฟรม)
  * แถวที่ 3: Jump (5 เฟรม)

2. การตั้งค่าใน Unity (สำคัญมากสำหรับภาพ Pixel Art)
- ลากโฟลเดอร์ Sprites เข้าไปใน Assets/ ของโปรเจกต์ Unity
- นำไฟล์ Scripts/Editor_BlueGreatswordKnightImporter.cs ไปวางใน Assets/Editor/
  (สคริปต์จะตั้งค่า Texture Type = Sprite Multiple, Filter Mode = Point (no filter), Compression = None ให้อัตโนมัติ)
- คลิกที่ไฟล์ Sprite Sheet -> กด Open Sprite Editor -> เลือก Slice -> Type: Automatic -> กด Slice -> กด Apply
- ลากเฟรมย่อยของแต่ละท่าลงในหน้าต่าง Scene เพื่อสร้าง Animation Clip (.anim) และ Animator Controller ได้ทันที
