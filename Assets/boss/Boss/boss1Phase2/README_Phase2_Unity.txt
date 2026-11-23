===================================================================
BLUE SOULFIRE GREATSWORD BOSS (PHASE 1 + PHASE 2) - UNITY ASSETS
===================================================================

1. ไฟล์แอนิเมชันเฟส 2 (Sprites/ - พื้นหลังโปร่งใส PNG พร้อมใช้งาน)
- BlueKnight_Phase2_Soulfire_FullSheet.png (รวมทุกท่าเฟส 2 ในแผ่นเดียว)
- แถวแยกตามชื่อท่า (นำเข้าและกด Automatic Slice ได้ทันที):
  * Phase2_01_Transition_9frames.png      : ท่าระเบิดเสาเพลิงสีฟ้าเข้าสู่เฟส 2 (9 เฟรม)
  * Phase2_02_Idle_5frames.png            : ท่ายืนหายใจพร้อมไฟลุกท่วมตัวและเกราะ (5 เฟรม)
  * Phase2_03_Run_5frames.png             : ท่าพุ่งตัวความเร็วสูงทิ้งภาพติดตา (5 เฟรม)
  * Phase2_04_Attack_Enhanced_7frames.png : ท่าฟันคอมโบหนัก + ปล่อยสายฟ้าเพลิงสีฟ้า (7 เฟรม)
  * Phase2_05_Ranged_Storm_5frames.png    : ท่ายิงพายุลูกไฟวิญญาณระยะไกล (5 เฟรม)
  * Phase2_06_Hurt_Rapid_3frames.png      : ท่าชะงักสั้นๆ แบบไม่เสียจังหวะ (3 เฟรม)
  * Phase2_07_Death_Explosive_9frames.png : ท่าตายแบบร่างสลายและระเบิดเปลวเพลิง (9 เฟรม)
  * Phase2_08_Sword_Spin_7frames.png      : ท่ากระโดดหมุนตัวฟันพายุดาบใหญ่ (7 เฟรม)

2. ระบบเกมเพลย์ในสคริปต์ BlueSoulfireBossController.cs
- Self-Burn Mechanic: เมื่อบอสเลือดต่ำกว่า 50% จะเล่นท่า Phase2_Transition และเริ่มเสีย HP ต่อเนื่องทุกวินาทีจากไฟที่เผาผลาญตัวเอง
- High-Speed Aggression: เพิ่ม anim.speed และความเร็วเคลื่อนที่ 1.45 เท่า พร้อมปลดล็อกท่าฟันสายฟ้า, ท่ายิงกระสุนเพลิง 5 ทิศทาง และท่าหมุนดาบกลางอากาศ
