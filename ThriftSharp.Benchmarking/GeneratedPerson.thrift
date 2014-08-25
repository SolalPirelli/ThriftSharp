// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

enum GeneratedHobby {
	Painting = 0,
	Drawing = 1,
	Sculpting = 2,
	Printmaking = 3,
	Ceramics = 4,
	StageDesign = 5,
	Writing = 6
}
        
struct GeneratedPerson {
	1: required string FirstName;
	2: optional list<string> MiddleNames;
	3: required string LastName;
	4: optional i32 Age;
	5: required bool IsAlive;
	6: required list<GeneratedHobby> Hobbies;
	7: optional string Description;
}