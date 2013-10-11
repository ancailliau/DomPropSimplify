using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DomPropSimplify.Tests
{
	[TestFixture()]
	public class TestParsing
	{
		// [TestCase("(!(anesthesia) & !(adaptive_imaging) & !(moulding) & !(outpatient) & !(move_by_patlog) & !(ct) & !(mr) & !(gtr) & fbr) | (!(anesthesia) & !(adaptive_imaging) & !(moulding) & !(outpatient) & !(move_by_patlog) & !(ct) & !(mr) & gtr & !(fbr)) | (!(anesthesia) & !(adaptive_imaging) & !(moulding) & !(outpatient) & move_by_patlog & !(ct) & !(gtr) & fbr) | (!(anesthesia) & !(adaptive_imaging) & !(moulding) & !(outpatient) & move_by_patlog & !(ct) & gtr & !(fbr)) | (!(anesthesia) & !(adaptive_imaging) & !(moulding) & !(outpatient) & move_by_patlog & ct & !(mr) & !(gtr) & fbr) | (!(anesthesia) & !(adaptive_imaging) & !(moulding) & !(outpatient) & move_by_patlog & ct & !(mr) & gtr & !(fbr)) | (!(anesthesia) & !(adaptive_imaging) & !(moulding) & outpatient & !(move_by_patlog) & !(ct) & mr & !(gtr) & !(fbr)) | (!(anesthesia) & !(adaptive_imaging) & !(moulding) & outpatient & !(move_by_patlog) & ct & !(mr) & !(gtr) & !(fbr)) | (!(anesthesia) & !(adaptive_imaging) & moulding & !(outpatient) & ct & !(mr) & !(gtr) & !(fbr)) | (!(anesthesia) & adaptive_imaging & !(moulding) & !(outpatient) & !(ct) & mr & !(gtr) & !(fbr)) | (!(anesthesia) & adaptive_imaging & !(moulding) & !(outpatient) & ct & !(mr) & !(gtr) & !(fbr)) | (anesthesia & !(adaptive_imaging) & !(moulding) & !(outpatient) & move_by_patlog & !(ct) & gtr & !(fbr))")]
		[TestCase("(!(anesthesia) & !(adaptive_imaging) & !(moulding) & outpatient) | (!(anesthesia) & !(adaptive_imaging) & moulding) | (!(anesthesia) & adaptive_imaging)")]
		[TestCase("(!(anesthesia) & !(adaptive_imaging) & !(moulding) & !(outpatient)) | (anesthesia)")]
		[TestCase("(!(adaptive_imaging) & moulding & move_by_patlog) | (adaptive_imaging & move_by_patlog)")]
		[TestCase("(!(adaptive_imaging)) | (adaptive_imaging & ct)")]
		[TestCase("adaptive_imaging & !(ct)")]
		[TestCase("!(anesthesia) & !(ct) & !(mr) & !(gtr)")]
		[TestCase("!(anesthesia) & ct")]
		[TestCase("!(ct) & !(mr) & gtr")]
		[TestCase("(!(anesthesia) & !(ct) & mr) | (anesthesia & mr)")]
		[TestCase("!(adaptive_imaging) & moulding")]
        [TestCase("!(adaptive_imaging) & !(moulding) & !(outpatient)")]
		[TestCase("anesthesia")]
		[TestCase("!(adaptive_imaging) & !(moulding) & !(outpatient) & move_by_patlog")]
		[TestCase("!(anesthesia)")]
		[TestCase("anesthesia")]
		[TestCase("(!(anesthesia) & !(adaptive_imaging) & !(moulding) & !(outpatient) & move_by_patlog) | (!(anesthesia) & !(adaptive_imaging) & moulding & move_by_patlog) | (!(anesthesia) & adaptive_imaging & move_by_patlog)")]
		[TestCase("!(adaptive_imaging) & !(moulding) & !(outpatient) & !(move_by_patlog) & gtr")]
		[TestCase("(!(adaptive_imaging) & !(moulding) & outpatient & ct) | (!(adaptive_imaging) & moulding & !(move_by_patlog)) | (adaptive_imaging & !(move_by_patlog) & ct)")]
	    [TestCase("(!(adaptive_imaging) & !(moulding) & outpatient & !(ct)) | (adaptive_imaging & !(move_by_patlog) & !(ct))")]
		[TestCase("(!(adaptive_imaging) & !(moulding) & outpatient) | (adaptive_imaging)")]
        [TestCase("!(adaptive_imaging) & !(moulding) & !(outpatient) & !(move_by_patlog) & !(gtr)")]
		[TestCase("(!(anesthesia) & !(adaptive_imaging) & !(moulding) & !(outpatient) & !(move_by_patlog)) | (!(anesthesia) & !(adaptive_imaging) & !(moulding) & outpatient) | (!(anesthesia) & !(adaptive_imaging) & moulding & !(move_by_patlog)) | (!(anesthesia) & adaptive_imaging & !(move_by_patlog))")]
        [TestCase("!(anesthesia) & !(gtr)")]
		[TestCase("(!(anesthesia) & gtr) | (anesthesia)")]
		public void TestCase (string input)
		{

		}
	}
}

