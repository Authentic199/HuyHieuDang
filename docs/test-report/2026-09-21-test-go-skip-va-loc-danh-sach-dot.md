# Báo cáo kiểm thử — nhánh gộp: gỡ Skip, nối lọc cho danh sách đợt, đồng bộ tài liệu

Lệnh đã chạy: `cd BE && dotnet test --logger trx`. Trước đó `cd BE && dotnet build` 0 lỗi, và phía Frontend `npm run build`, `npm run lint`, `npm run typecheck:e2e`, `npm run format:check` đều xanh.

Tổng: 733 đạt, 0 hỏng, 1 bỏ qua trên 734 ca.

So với lần chạy trên `main` sáng nay (684 đạt / 7 bỏ qua): thêm 49 ca chạy thật, và số ca bị `Skip` rơi từ 7 xuống 1. Ca `Skip` duy nhất còn lại là QC-05, loại trừ có chủ ý theo quyết định của CEO.

Sáu ca `A8DefectTests` trước đây nằm sau `Skip` nay chạy thật. Hai trong sáu ca đó đỏ lúc mới gỡ — `QcT2705` vì `GET /api/AwardPeriods` không đọc bộ lọc, `QcT2707` vì bảng khóa của bộ kiểm thử còn là bản v1.3 — cả hai đã được sửa trong nhánh này và nay xanh.

## Qt3bAdmissionDateRangeTests

- GetAdmissionDateRange ForNone HasNoLowerBound — ĐẠT
- GetAdmissionDateRange AgreesWithGetNextMilestoneAroundLeapDays (year: 2026, month: 2, day: 28) — ĐẠT
- GetLatestAdmissionDateForAge WhenTodayIs28February IncludesTheLeapDay — ĐẠT
- GetLatestAdmissionDateForAge WhenTodayIs29February LandsOn28FebruaryOfANonLeapYear — ĐẠT
- GetAdmissionDateRange ForAMiddleMilestone IsHalfOpen — ĐẠT
- GetLatestAdmissionDateForAge WhenAgeIsZero ReturnsToday — ĐẠT
- GetAdmissionDateRange WhenMilestoneIsNotInTheSequence Throws — ĐẠT
- GetAdmissionDateRange AgreesWithGetNextMilestoneAroundLeapDays (year: 2024, month: 2, day: 29) — ĐẠT
- GetAdmissionDateRange AgreesWithGetNextMilestoneAroundLeapDays (year: 2027, month: 1, day: 1) — ĐẠT
- GetAdmissionDateRange FollowsTheMilestoneSettings — ĐẠT
- GetLatestAdmissionDateForAge WhenAgeIsNegative Throws — ĐẠT
- GetAdmissionDateRange AgreesWithGetNextMilestoneAroundLeapDays (year: 2026, month: 12, day: 31) — ĐẠT
- GetLatestAdmissionDateForAge SubtractsWholeYears — ĐẠT
- GetAdmissionDateRange AgreesWithGetNextMilestoneOnEveryFixtureMember — ĐẠT
- GetAdmissionDateRange ForTheFirstMilestone HasNoUpperBound — ĐẠT
- GetLatestAdmissionDateForAge WhenTodayIs28FebruaryOfALeapYear ExcludesTheLeapDay — ĐẠT

## Qt3aNextMilestoneTests

- GetNextMilestone WhenPartyAgeIsAboveTheHighestMilestone ReturnsNull — ĐẠT
- GetNextMilestone WhenPartyAgeIsJustUnderAMilestone ReturnsThatMilestone — ĐẠT
- GetPartyAge MatchesTheExpectedMemberListOfTheFixtures — ĐẠT
- GetNextMilestone WithStepTen SkipsTheMilestonesThatDisappear — ĐẠT
- GetNextAnniversary WhenPartyAgeIsAboveTheHighestMilestone ReturnsNull — ĐẠT
- GetNextMilestone WhenPartyAgeEqualsTheHighestMilestone ReturnsNull — ĐẠT
- GetNextMilestone MatchesTheExpectedMemberListOfTheFixtures — ĐẠT
- GetNextAnniversary ForAMemberBelowTheFirstMilestone ReturnsTheMilestoneDate — ĐẠT
- GetNextMilestone WhenPartyAgeEqualsAMilestone ReturnsTheFollowingMilestone — ĐẠT
- GetNextMilestone WhenPartyAgeIsBelowTheFirstMilestone ReturnsTheFirstMilestone — ĐẠT

## Qt4EligibilityTests

- GetEligibleMilestone WithStepTen KeepsTheMemberWhoseMilestoneSurvives — ĐẠT
- GetEligibleMilestone OverTheWholeCoreDataset MatchesTheExpectedCounts (scenarioName: "core_step10_T0") — ĐẠT
- GetEligibleMilestone OverTheWholeCoreDataset MatchesTheExpectedCounts (scenarioName: "core_default_T0") — ĐẠT
- GetEligibleMilestone WhenThePeriodStartsOn29FebruaryOfANonLeapYear BindsTo28February — ĐẠT
- GetEligibleMilestone WhenTheMemberHasPassedTheHighestMilestone ReturnsNull — ĐẠT
- GetEligibleMilestone WhenLeapDayAnniversaryShiftsTo28February StillFallsInThePeriod — ĐẠT
- GetEligibleMilestone WhenAnniversaryEqualsTheFirstDayOfThePeriod ReturnsTheMilestone — ĐẠT
- GetEligibleMilestone ForTheMemberAtTheHighestMilestone StillReturns90 — ĐẠT
- GetEligibleMilestone WithStepTen LosesTheMemberWhoseMilestoneDisappears — ĐẠT
- GetEligibleMilestone OverTheWholeCoreDataset MatchesTheExpectedCounts (scenarioName: "core_default_T0_overlap") — ĐẠT
- GetEligibleMilestone WhenAnniversaryIsOneDayBeforeThePeriod ReturnsNull — ĐẠT
- GetEligibleMilestone OverTheWholeCoreDataset MatchesTheExpectedCounts (scenarioName: "core_default_T0_fullCover") — ĐẠT
- GetEligibleMilestone OverTheWholeCoreDataset MatchesTheExpectedCounts (scenarioName: "core_default_T0_widenedP3") — ĐẠT
- GetEligibleMilestone ForAYearWithoutAnyMilestone ReturnsNull — ĐẠT
- GetEligibleMilestone WhenAnniversaryIsOneDayAfterThePeriod ReturnsNull — ĐẠT
- GetEligibleMilestone WhenAnniversaryEqualsTheLastDayOfThePeriod ReturnsTheMilestone — ĐẠT
- GetEligibleMilestone OverTheWholeCoreDataset MatchesTheExpectedCounts (scenarioName: "core_default_T0_leapEdgePeriod") — ĐẠT

## Qt6PeriodWarningTests

- GetOverlaps WhenPeriodsAreBackToBack ReturnsNoWarning — ĐẠT
- GetOverlaps WhenTwoPeriodsIntersect ReturnsThePairInOrder — ĐẠT
- GetGaps WhenThereIsNoPeriod LabelsTheWholeYearAsBeforeTheFirstPeriod — ĐẠT
- GetGaps ForTheMainPeriods LabelsTheFirstAndLastGapByPosition — ĐẠT
- GetGaps WhenThereIsNoPeriod ReturnsTheWholeYearAsOneGap — ĐẠT
- BindToYear WhenTheDayOrMonthDoesNotExist ThrowsBadRequest (day: 0, month: 1) — ĐẠT
- GetGaps WhenThePeriodsCoverTheWholeYear ReturnsNoGap — ĐẠT
- BindToYear WhenTheDayOrMonthDoesNotExist ThrowsBadRequest (day: 31, month: 2) — ĐẠT
- GetGaps ForTheMainPeriods ReturnsTheFiveExpectedGapsOf2026 — ĐẠT
- BindToYear WhenTheFromDateIsAfterTheToDate EndsThePeriodInTheNextYear — ĐẠT
- BindToYear WhenTheDayOrMonthDoesNotExist ThrowsBadRequest (day: 32, month: 1) — ĐẠT
- GetGaps InALeapYear EndsTheGapOnTheCorrectFebruaryDay — ĐẠT
- BindToYear WhenThePeriodStartsOn29February IsStillAccepted — ĐẠT
- BindToYear WhenTheDayOrMonthDoesNotExist ThrowsBadRequest (day: 30, month: 2) — ĐẠT
- BindToYear WhenTheDayOrMonthDoesNotExist ThrowsBadRequest (day: 1, month: 13) — ĐẠT
- GetOverlaps ForTheMainPeriods ReturnsNoWarning — ĐẠT
- BindToYear WhenTheDayOrMonthDoesNotExist ThrowsBadRequest (day: 31, month: 4) — ĐẠT
- GetOverlaps WhenTwoPeriodsShareASingleDay ReportsThem — ĐẠT

## Qt6SpanningYearPeriodTests

- GetEligibleMilestone WhenTheAnniversaryFallsInTheFirstCalendarYear ReturnsTheMilestone — ĐẠT
- GetPeriodStatus ForAYearFarInTheFuture StillBindsToThatYear — ĐẠT
- GetPeriodStatus OnADayBetweenTwoOccurrences CountsDownToTheNextFromDate — ĐẠT
- BindToYear ForASpanningPeriod PutsTheToDateInTheFollowingYear — ĐẠT
- GetUpcomingPeriod WhenTheSpanningPeriodStartedLastYear PicksThatOccurrence — ĐẠT
- GetSlicesInYear ForASpanningPeriod ReturnsTheTailAndTheHeadOfTheYear — ĐẠT
- GetMissedMilestone WhenTheAnniversaryFallsInTheTailOfTheSpanningPeriod ReturnsNull — ĐẠT
- GetGaps ForASpanningPeriodAlone LeavesOnlyTheMiddleOfTheYearUncovered — ĐẠT
- GetUpcomingPeriod WhenTheSpanningPeriodOfLastYearHasEnded PicksThisYearOccurrence — ĐẠT
- GetEligibleMilestone WhenTheAnniversaryFallsInTheGapOfTheYear ReturnsNull — ĐẠT
- BindToYear ForASpanningPeriodEndingOn29February KeepsTheLeapDayInALeapYear — ĐẠT
- GetOverlaps ForASpanningPeriodAlone DoesNotReportThePeriodAgainstItself — ĐẠT
- GetOverlaps WhenAnotherPeriodTouchesTheTailOfTheSpanningPeriod ReportsTheSharedDays — ĐẠT
- GetEligibleMilestone WhenTheAnniversaryFallsAfterTheNewYear ReturnsTheMilestone — ĐẠT
- GetGaps WhenTwoSpanningPeriodsCoverTheWholeYear ReturnsNoGap — ĐẠT
- GetPeriodStatus OnADayInsideTheOccurrenceStartedLastYear IsOngoing — ĐẠT
- GetMissedMilestone WhenTheAnniversaryFallsInTheMiddleOfTheYear ReturnsTheGap — ĐẠT

## Qt11PeriodStatusTests

- GetPeriodStatus OneDayBeforeThePeriodStarts ReturnsOneDayLeft — ĐẠT
- GetPeriodStatus ForEveryFixtureScenario MatchesTheExpectedStatuses (scenarioName: "core_default_T0_overlap") — ĐẠT
- GetPeriodStatus OnTheFirstDayOfThePeriod ReturnsOngoing — ĐẠT
- GetPeriodStatus ForAPeriodStartingOn29FebruaryOfANonLeapYear BindsTo28February — ĐẠT
- GetPeriodStatus OneDayAfterThePeriodEnds ReturnsPast — ĐẠT
- GetPeriodStatus ForEveryFixtureScenario MatchesTheExpectedStatuses (scenarioName: "core_default_T0_fullCover") — ĐẠT
- BindToYear ForAPeriodEndingOn29February KeepsTheDayInALeapYear — ĐẠT
- GetPeriodStatus ForEveryFixtureScenario MatchesTheExpectedStatuses (scenarioName: "core_default_T0_leapEdgePeriod") — ĐẠT
- GetPeriodStatus WhenThePeriodHasNotStarted ReturnsUpcomingWithDaysLeft — ĐẠT
- GetPeriodStatus WhenThePeriodAlreadyEnded ReturnsPastWithoutDaysLeft — ĐẠT
- GetPeriodStatus ForEveryFixtureScenario MatchesTheExpectedStatuses (scenarioName: "core_default_T0_widenedP3") — ĐẠT
- GetPeriodStatus WhenTodayIsInsideThePeriod ReturnsOngoingWithoutDaysLeft — ĐẠT
- GetPeriodStatus ForEveryFixtureScenario MatchesTheExpectedStatuses (scenarioName: "core_default_T0") — ĐẠT
- GetPeriodStatus OnTheLastDayOfThePeriod ReturnsOngoing — ĐẠT
- GetPeriodStatus ForEveryFixtureScenario MatchesTheExpectedStatuses (scenarioName: "core_default_T2") — ĐẠT
- GetPeriodStatus ForEveryFixtureScenario MatchesTheExpectedStatuses (scenarioName: "core_default_T1") — ĐẠT

## Qt8UpcomingPeriodTests

- GetUpcomingPeriod WhenThereIsNoPeriod ReturnsNull — ĐẠT
- GetUpcomingPeriod WhenTwoPeriodsShareTheSameStartDate DoesNotDependOnTheInputOrder — ĐẠT
- GetUpcomingPeriod AtT0 ReturnsTheNextPeriodOfTheCurrentYearWithDaysLeft — ĐẠT
- GetUpcomingPeriod OnTheLastDayOfAPeriod StillReturnsThatPeriodAsOngoing — ĐẠT
- GetUpcomingPeriod ForEveryFixtureScenario MatchesTheExpectedPeriod (scenarioName: "core_default_T0_overlap") — ĐẠT
- GetUpcomingPeriod OneDayAfterTheLastPeriodEnds RollsOverToNextYear — ĐẠT
- GetUpcomingPeriod ForEveryFixtureScenario MatchesTheExpectedPeriod (scenarioName: "core_default_T1") — ĐẠT
- GetUpcomingPeriod WhenEveryPeriodOfTheYearHasPassed ReturnsTheEarliestOfNextYear — ĐẠT
- GetUpcomingPeriod ForEveryFixtureScenario MatchesTheExpectedPeriod (scenarioName: "core_default_T0_widenedP3") — ĐẠT
- GetUpcomingPeriod ForEveryFixtureScenario MatchesTheExpectedPeriod (scenarioName: "core_default_T0_noPeriod") — ĐẠT
- GetUpcomingPeriod ForEveryFixtureScenario MatchesTheExpectedPeriod (scenarioName: "core_default_T2") — ĐẠT
- GetUpcomingPeriod WhenTwoPeriodsOverlap PicksTheOneWithTheEarliestStart — ĐẠT
- GetUpcomingPeriod ForEveryFixtureScenario MatchesTheExpectedPeriod (scenarioName: "core_default_T0_leapEdgePeriod") — ĐẠT
- GetUpcomingPeriod WhenTwoPeriodsShareTheSameStart PrefersTheEarlierEndThenTheName — ĐẠT
- GetUpcomingPeriod ForEveryFixtureScenario MatchesTheExpectedPeriod (scenarioName: "core_default_T0_fullCover") — ĐẠT
- GetUpcomingPeriod WhenEveryPeriodOfThisYearHasPassed KeepsTheSameTieBreakNextYear — ĐẠT
- GetUpcomingPeriod WhenTodayIsInsideAPeriod ReturnsThatPeriodAsOngoing — ĐẠT
- GetUpcomingPeriod ForEveryFixtureScenario MatchesTheExpectedPeriod (scenarioName: "core_default_T0") — ĐẠT
- GetUpcomingPeriod ForAPeriodStartingOn29February BindsNextYearTo28February — ĐẠT

## Qt1MilestoneSequenceTests

- BuildMilestones WithAStepLargerThanTheRange ReturnsOnlyTheFirstMilestone — ĐẠT
- BuildMilestones WhenStepIsLessThanOne ThrowsBadRequest (step: 0) — ĐẠT
- BuildMilestones WithStepTen Returns7Milestones — ĐẠT
- BuildMilestones WhenStartIsGreaterThanEnd ThrowsBadRequest — ĐẠT
- BuildMilestones WhenAnyBoundIsNotPositive ThrowsBadRequest (start: -30, end: 90, step: 5) — ĐẠT
- BuildMilestones WhenStepDoesNotLandOnEnd StopsBeforeExceedingEnd — ĐẠT
- BuildMilestones WhenAnyBoundIsNotPositive ThrowsBadRequest (start: 30, end: 0, step: 5) — ĐẠT
- BuildMilestones WhenStepIsLessThanOne ThrowsBadRequest (step: -1) — ĐẠT
- BuildMilestones WhenAnyBoundIsNotPositive ThrowsBadRequest (start: 0, end: 90, step: 5) — ĐẠT
- BuildMilestones WhenAnyBoundIsNotPositive ThrowsBadRequest (start: 30, end: -90, step: 5) — ĐẠT
- BuildMilestones WithDefaultSettings Returns13MilestonesFrom30To90 — ĐẠT
- BuildMilestones WhenStartEqualsEnd ReturnsSingleMilestone — ĐẠT

## Qt3PartyAgeTests

- GetPartyAge BeforeThisYearAnniversary DoesNotCountTheCurrentYear — ĐẠT
- GetPartyAge ForTheOldestMember ReturnsAgeBeyondTheHighestMilestone — ĐẠT
- GetPartyAge OnTheAnniversaryDay CountsThatYear — ĐẠT
- GetPartyAge OneDayBeforeTheAnniversary ReturnsPreviousCount — ĐẠT
- GetPartyAge WhenAdmittedToday ReturnsZero — ĐẠT
- GetPartyAge WhenAdmissionIsInTheFuture ReturnsZero — ĐẠT
- GetPartyAge ForLeapDayAdmission CountsOn28FebruaryOfNonLeapYear — ĐẠT
- GetPartyAge ForLeapDayAdmission OneDayBefore28February DoesNotCountTheYear — ĐẠT

## Qt11PeriodStatusByYearTests

- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026(fromDay: 15, fromMonth: 1, toDay: 5, toMonth: 3, expected: Past, daysLeft: null) — ĐẠT
- QT11 · Quá tải không tham số năm vẫn xét đúng năm hiện tại — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026(fromDay: 1, fromMonth: 10, toDay: 7, toMonth: 11, expected: Upcoming, daysLeft: 12) — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026(fromDay: 1, fromMonth: 9, toDay: 30, toMonth: 9, expected: Ongoing, daysLeft: null) — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026(fromDay: 19, fromMonth: 9, toDay: 19, toMonth: 9, expected: Ongoing, daysLeft: null) — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026(fromDay: 15, fromMonth: 8, toDay: 10, toMonth: 9, expected: Past, daysLeft: null) — ĐẠT
- QT11 · Đợt 29/02 ở năm không nhuận xét theo 28/02 — ĐẠT
- QT11 · Năm đã qua thì mọi đợt là Đã qua, năm sau thì là Sắp tới — ĐẠT

## Qt2AnniversaryTests

- GetAnniversary ForOrdinaryDate KeepsDayAndMonth — ĐẠT
- GetAnniversary WhenAdmittedOn29FebruaryAndTargetYearIsNotLeap FallsBackTo28February — ĐẠT
- GetAnniversary WithMilestoneZero ReturnsAdmissionDate — ĐẠT
- GetAnniversary WhenMilestoneIsNegative ThrowsBadRequest — ĐẠT
- GetAnniversary WhenAdmittedOn29FebruaryAndTargetYearIsLeap Keeps29February — ĐẠT

## Qt7MissedMilestoneTests

- GetMissedMilestone OverTheWholeCoreDataset MatchesTheExpectedRows (year: 2025) — ĐẠT
- GetMissedMilestone WhenTheAnniversaryFallsAfterTheLastPeriod LabelsItAfterTheLast — ĐẠT
- GetMissedMilestone WhenTheAnniversaryFallsInsideAPeriod ReturnsNull — ĐẠT
- GetMissedMilestone WhenTheAnniversaryFallsBetweenTwoPeriods NamesBothPeriods — ĐẠT
- GetMissedMilestone WhenThereIsNoPeriodAtAll ReturnsTheWholeYearGap — ĐẠT
- GetMissedMilestone WhenNoMilestoneFallsInTheYear ReturnsNull — ĐẠT
- GetMissedMilestone WithStepTen DropsTheMemberWhoseMilestoneDisappears — ĐẠT
- GetMissedMilestone WhenTheAnniversaryFallsBeforeTheFirstPeriod LabelsItBeforeTheFirst — ĐẠT
- GetMissedMilestone OverTheWholeCoreDataset MatchesTheExpectedRows (year: 2027) — ĐẠT
- GetMissedMilestone OverTheWholeCoreDataset MatchesTheExpectedRows (year: 2028) — ĐẠT
- GetMissedMilestone OverTheWholeCoreDataset MatchesTheExpectedRows (year: 2026) — ĐẠT

## Qc07Qt7MissedMilestoneTests

- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch(scenario: "core_default_T0_leapEdgePeriod", periodSet: "leapEdge") — ĐẠT
- U-712 · Không cài đợt nào thì 27 người của bộ lõi đều bị sót ở 2026 — ĐẠT
- QC · Người bị sót và người đủ điều kiện là hai tập rời nhau — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json(scenario: "core_default_T0_overlap", periodSet: "overlap") — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json(scenario: "core_default_T0_leapEdgePeriod", periodSet: "leapEdge") — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch(scenario: "core_default_T0_widenedP3", periodSet: "widened") — ĐẠT
- U-710 · Đổi Bước sang 10 thì S04 rời danh sách bị sót, còn 6 người — ĐẠT
- U-711 · Bộ đợt phủ kín cả năm thì không ai bị sót — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch(scenario: "core_default_T0_overlap", periodSet: "overlap") — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json(scenario: "core_default_T0_widenedP3", periodSet: "widened") — ĐẠT
- QC-02 · Nhãn khoảng trống khi chưa cài đợt nào phải khớp expected.json — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch(scenario: "core_default_T0_noPeriod", periodSet: "none") — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json(scenario: "core_default_T0", periodSet: "main") — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch(scenario: "core_default_T0_fullCover", periodSet: "fullCover") — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch(scenario: "core_default_T0", periodSet: "main") — ĐẠT

## Qc01Qt1MilestoneTests

- U-101/U-102 · Dãy mốc khớp expected.json của QC(scenario: "core_default_T0") — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 30, end: 30, step: 5) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 1, end: 100, step: 1) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 30, end: 90, step: 5) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 5, end: 100, step: 7) — ĐẠT
- U-101 · Dãy mốc luôn tăng dần và không trùng — ĐẠT
- QC · Gọi hai lần cùng cài đặt cho hai danh sách bằng nhau và tách rời nhau — ĐẠT
- U-110 · 1 / 100 / 1 cho đúng 100 mốc, không treo — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 1, end: 1, step: 1) — ĐẠT
- U-106 · Mốc cuối đúng bằng Kết thúc thì phải có trong dãy — ĐẠT
- QC-04 · Bước = int.MaxValue chỉ được cho ra mốc đầu, không được tràn số — ĐẠT
- U-104 · Bước lớn hơn cả khoảng 30–90 thì chỉ còn mốc đầu — ĐẠT
- U-107/U-108/U-109 · Cài đặt sai bị từ chối bằng lỗi nghiệp vụ, không trả dãy rỗng — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 30, end: 90, step: 10) — ĐẠT
- U-101/U-102 · Dãy mốc khớp expected.json của QC(scenario: "core_step10_T0") — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 30, end: 95, step: 5) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 30, end: 90, step: 61) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 30, end: 92, step: 5) — ĐẠT

## Qc06Qt6PeriodTests

- U-606/U-607 · Ngày/tháng không tồn tại phải báo lỗi nghiệp vụ — ĐẠT
- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét(year: 2028) — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày(fromDay: 1, fromMonth: 10, toDay: 1, toMonth: 10, year: 2026, from: "2026-10-01", to: "2026-10-01") — ĐẠT
- U-609/U-611/U-612 · Cảnh báo chồng lấn khớp oracle của QC — ĐẠT
- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét(year: 2027) — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày(fromDay: 29, fromMonth: 2, toDay: 5, toMonth: 3, year: 2028, from: "2028-02-29", to: "2028-03-05") — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày(fromDay: 1, fromMonth: 10, toDay: 7, toMonth: 11, year: 2026, from: "2026-10-01", to: "2026-11-07") — ĐẠT
- U-609 · Hai đợt chồng lấn vẫn được tính bình thường, chỉ là cảnh báo — ĐẠT
- QC · Khoảng trống khớp oracle độc lập của QC trên mọi bộ đợt, 2024–2030 — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày(fromDay: 1, fromMonth: 1, toDay: 31, toMonth: 12, year: 2026, from: "2026-01-01", to: "2026-12-31") — ĐẠT
- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét(year: 2026) — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày(fromDay: 29, fromMonth: 2, toDay: 5, toMonth: 3, year: 2026, from: "2026-02-28", to: "2026-03-05") — ĐẠT
- U-602 · Đợt có Từ ngày > Đến ngày kết thúc ở năm sau — ĐẠT
- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét(year: 2025) — ĐẠT
- QC · Các khoảng trống không chồng nhau, không thủng, phủ đúng phần còn lại của năm — ĐẠT
- U-602b · Oracle của QC và service của Backend khớp nhau trên đợt vắt năm — ĐẠT

## Qc03Qt3PartyAgeTests

- QC · Tuổi đảng khớp oracle của QC trên mọi ngày 2024–2030 của cả bộ lõi — ĐẠT
- U-301/U-302/U-303 · Tuổi đảng quanh đúng ngày kỷ niệm tại T0(year: 1996, month: 9, day: 19, expected: 30) — ĐẠT
- U-304/U-305/U-306 · Các mốc tuổi đảng đặc biệt của bộ lõi tại T0(code: "V01", expected: 0) — ĐẠT
- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước(code: "M01", step: 10) — ĐẠT
- U-301/U-302/U-303 · Tuổi đảng quanh đúng ngày kỷ niệm tại T0(year: 1996, month: 9, day: 20, expected: 29) — ĐẠT
- U-307/U-308/U-309 · Người vào Đảng 29/02 xét ở năm không nhuận(y: 2026, m: 3, d: 1, expected: 30) — ĐẠT
- U-304/U-305/U-306 · Các mốc tuổi đảng đặc biệt của bộ lõi tại T0(code: "M01", expected: 91) — ĐẠT
- U-355 · L02 (29/02/1988) có mốc kế tiếp 40 rơi đúng 29/02/2028 — ĐẠT
- U-356 · Mốc kế tiếp phải LỚN HƠN tuổi đảng, không được bằng — ĐẠT
- U-307/U-308/U-309 · Người vào Đảng 29/02 xét ở năm không nhuận(y: 2026, m: 2, d: 27, expected: 29) — ĐẠT
- QC · Mốc kế tiếp khớp oracle của QC trên cả bộ lõi ở T0/T1/T2, hai cài đặt — ĐẠT
- QC · Tuổi đảng chỉ tăng theo thời gian, không bao giờ tụt (quét bộ lõi 2024–2030) — ĐẠT
- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước(code: "M01", step: 5) — ĐẠT
- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước(code: "M02", step: 10) — ĐẠT
- U-301/U-302/U-303 · Tuổi đảng quanh đúng ngày kỷ niệm tại T0(year: 1996, month: 9, day: 18, expected: 30) — ĐẠT
- U-307/U-308/U-309 · Người vào Đảng 29/02 xét ở năm không nhuận(y: 2026, m: 2, d: 28, expected: 30) — ĐẠT
- U-304/U-305/U-306 · Các mốc tuổi đảng đặc biệt của bộ lõi tại T0(code: "M02", expected: 90) — ĐẠT
- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước(code: "M02", step: 5) — ĐẠT

## Qc12Qt6SpanningYearTests

- U-623 · Người tròn mốc đúng 01/12 và đúng 28/02 đều đủ điều kiện — ĐẠT
- U-621 · Đợt 02/01 – 01/01 phủ trọn năm: không mốc trùng, không vòng lặp vô hạn — ĐẠT
- U-624b · Hai đợt vắt năm: cặp chồng lấn hiện đúng hai đoạn, đầu năm và cuối năm — ĐẠT
- U-622 · Đợt vắt năm kết thúc 29/02 lùi về 28/02 khi năm kết thúc không nhuận(year: 2026, from: "2026-12-01", to: "2027-02-28") — ĐẠT
- U-625 · Xóa đợt vắt năm thì khoảng trống mới phủ đúng hai đầu năm — ĐẠT
- U-622 · Đợt vắt năm kết thúc 29/02 lùi về 28/02 khi năm kết thúc không nhuận(year: 2028, from: "2028-12-01", to: "2029-02-28") — ĐẠT
- U-622 · Đợt vắt năm kết thúc 29/02 lùi về 28/02 khi năm kết thúc không nhuận(year: 2027, from: "2027-12-01", to: "2028-02-29") — ĐẠT
- U-626 · Service và oracle độc lập của QC khớp nhau trên mọi bộ đợt vắt năm, 2024–2030 — ĐẠT
- U-624a · Đợt vắt năm chồng lấn một đợt thường ở đầu năm: đúng một cặp, đúng khoảng ngày — ĐẠT
- U-620 · Đợt 01/12 – 30/11 dài 365 ngày vẫn chỉ trao tối đa 1 mốc/người/đợt — ĐẠT

## Qc10PerformanceTests

- U-1201 · Tính đủ điều kiện 1 đợt / 1 năm cho 10.000 đảng viên dưới 1 giây — ĐẠT
- QC · Quét 'chưa thuộc đợt nào' cho 10.000 đảng viên dưới 1 giây — ĐẠT

## Qc08Qt8UpcomingPeriodTests

- QC · Đợt sắp tới khớp oracle của QC ở mọi ngày của 2026 và 2028 — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-01-01", name: "Đợt 3/2", year: 2026) — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-12-01", name: "Đợt 3/2", year: 2027) — ĐẠT
- U-809 · Đợt 29/02 sang năm không nhuận: thu về 28/02 và đếm ngày theo ngày đã thu — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-11-07", name: "Đợt 7/11", year: 2026) — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-12-31", name: "Đợt 3/2", year: 2027) — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-10-15", name: "Đợt 7/11", year: 2026) — ĐẠT
- U-808 · Hai đợt cùng Từ ngày phải cho kết quả tất định — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-09-19", name: "Đợt 7/11", year: 2026) — ĐẠT
- U-807 · Chưa cài đợt nào thì không có đợt sắp tới — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-11-08", name: "Đợt 3/2", year: 2027) — ĐẠT
- QC · Đợt sắp tới luôn có Đến ngày không nhỏ hơn hôm nay — ĐẠT

## Qc02Qt2AnniversaryTests

- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 1) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 3) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 5) — ĐẠT
- U-208 · Mốc 0 trả đúng ngày vào Đảng — ĐẠT
- U-207 · 31/01/1996 + 30 năm → 31/01/2026, ngày cuối tháng không bị đụng — ĐẠT
- QC · Đối chiếu mọi ngày của 4 năm (nhuận và không nhuận) với oracle của QC — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 8) — ĐẠT
- U-204 · 29/02/1996 + 4 năm → 2000 chia hết 400 nên vẫn nhuận — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 7) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 10) — ĐẠT
- U-206 · 28/02/1996 + 32 năm → 28/02/2028, không được nhảy sang 29/02 — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 12) — ĐẠT
- U-205 · 29/02/1896 + 4 năm → 1900 chia hết 100 nhưng KHÔNG nhuận → 28/02 — ĐẠT

## Qc04Qt4EligibilityTests

- U-416 · Phân bổ mốc của Đợt 7/11 năm 2026: 30×3, 35×1, 40×1, 45×1 — ĐẠT
- U-418 · Không ai đủ điều kiện ở hai đợt cùng một năm — ĐẠT
- U-411/U-412 · N01 chỉ đủ điều kiện Đợt 7/11 của năm 2027, không phải 2026 — ĐẠT
- U-413/U-414 · Đợt có Từ ngày 29/02: thu về 28/02 ở 2026, giữ 29/02 ở 2028 — ĐẠT
- U-416/U-417 · Số người đủ điều kiện Đợt 7/11 năm 2026 theo Bước 5 và Bước 10(step: 10, expectedCount: 4) — ĐẠT
- QC · Danh sách đủ điều kiện từng đợt/từng năm khớp expected.json của QC(scenario: "core_default_T0") — ĐẠT
- U-416/U-417 · Số người đủ điều kiện Đợt 7/11 năm 2026 theo Bước 5 và Bước 10(step: 5, expectedCount: 6) — ĐẠT
- QC · Đủ điều kiện khớp oracle của QC trên mọi đợt × mọi năm 2020–2035 — ĐẠT
- U-408/U-409 · L02 chỉ đủ điều kiện ở năm nhuận 2028 với mốc 40 — ĐẠT
- U-415 · Đợt một ngày (Từ = Đến = 01/10) chỉ nhận đúng người tròn mốc hôm đó — ĐẠT
- QC · Danh sách đủ điều kiện từng đợt/từng năm khớp expected.json của QC(scenario: "core_step10_T0") — ĐẠT

## Qc09Qt11PeriodStatusTests

- QC · Chỉ trạng thái Sắp tới mới có số ngày còn lại, và luôn dương — ĐẠT
- QC · Trạng thái và số ngày còn lại khớp oracle của QC ở mọi ngày 2026–2028 — ĐẠT
- U-1102/U-1107 · Số ngày còn lại đếm đúng từng ngày trước Từ ngày của Đợt 7/11 — ĐẠT
- QC · Trạng thái 4 đợt chính khớp expected.json ở T0, T1, T2(scenario: "core_default_T1") — ĐẠT
- QC · Trạng thái 4 đợt chính khớp expected.json ở T0, T1, T2(scenario: "core_default_T2") — ĐẠT
- QC · Trạng thái 4 đợt chính khớp expected.json ở T0, T1, T2(scenario: "core_default_T0") — ĐẠT

## Qc11ClockScanTests

- A-901 · Module nghiệp vụ trong Infrastructure không đọc đồng hồ (trừ 7 dòng tầng khung) — ĐẠT
- A-901 · Không nơi nào trong BE/src đọc đồng hồ theo giờ máy (T-FIX-2) — ĐẠT
- QC-05 · Core không được đọc đồng hồ hệ thống — BỎ QUA
- A-901 · Service tính mốc tuổi đảng không đọc đồng hồ, chỉ nhận today qua tham số — ĐẠT

## Qc05Qt5NoStoredResultTests

- U-501 · Service không sửa danh sách đợt và danh sách mốc được truyền vào — ĐẠT
- U-503 · Không có bảng/entity nào lưu danh sách đủ điều kiện — ĐẠT
- U-501 · Gọi hai lần với cùng dữ liệu cho cùng kết quả, không tác dụng phụ — ĐẠT
- U-502 · Nới Đến ngày của đợt thì người đang bị sót chuyển sang đủ điều kiện ngay — ĐẠT
- U-502 · Đổi cài đặt giữa hai lần gọi thì lần thứ hai đổi theo ngay — ĐẠT

## SkeletonIntegrationTests

- DatabaseSettings ShouldDefaultToEmptyConnectionString — ĐẠT

## QueryFilterValidationTests

- BanIEnumerable CungHanhVi — ĐẠT
- GiaTriEnumSaiKieu Tra400 (value: "$eq:1") — ĐẠT
- KhongCoBoLoc GiuNguyenDanhSach — ĐẠT
- GiaTriSoSaiKieu Tra400 — ĐẠT
- ToanTuNullTrenTruongCoTheNull VanLocDung — ĐẠT
- ToanTuIlikeVaSwTrenChuoi VanLocDung — ĐẠT
- ToanTuIlikeTrenTruongKhongPhaiChuoi Tra400 — ĐẠT
- ToanTuBtwTrenNgayHopLe VanLocDung — ĐẠT
- ToanTuKhongTonTaiHoacThieuToanTu Tra400 — ĐẠT
- GiaTriEnumSaiKieu Tra400 (value: "$eq:Khac") — ĐẠT
- GiaTriEnumSaiKieu Tra400 (value: "$not:$eq:Khac") — ĐẠT
- TenTruongKhongTonTai GiuNguyenHanhViCu — ĐẠT
- TienToNotVoiGiaTriHopLe VanLocDung — ĐẠT
- ToanTuInHopLe VanLocDung — ĐẠT
- DanhSachRong VanTra400ChoGiaTriLa — ĐẠT
- GiaTriEnumSaiKieu Tra400 (value: "$in:Male,Khac") — ĐẠT
- GiaTriEnumSaiKieu Tra400 (value: "$eq:") — ĐẠT
- ToanTuInTrenTruongCoTheNull VanLocDung — ĐẠT
- ToanTuNullTrenTruongKhongTheNull Tra400 — ĐẠT
- GiaTriEnumHopLe VanLocDung — ĐẠT
- ToanTuBtwHopLe VanLocDung — ĐẠT

## BusinessSchemaTests

- AppSetting ShouldDefaultTo30 90 5 WithoutUnitName — ĐẠT
- Gender ShouldOnlyOfferMaleAndFemale — ĐẠT
- PartyMember DateColumns ShouldBeDateOnly — ĐẠT
- AwardPeriod Name ShouldBeUniqueAndCaseInsensitive — ĐẠT
- BusinessEntities ShouldMapToUnderscoreTables (entityType: typeof(HuyHieuDang.Infrastructure.Modules.AppSettings.Entities.AppSetting), tableName: "app_setting") — ĐẠT
- AwardPeriod ShouldNotStoreAnyYear — ĐẠT
- Model ShouldNotContainAnyEligibilityResultTable — ĐẠT
- BusinessEntities ShouldMapToUnderscoreTables (entityType: typeof(HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod), tableName: "award_period") — ĐẠT
- PartyMember OfficialAdmissionDate ShouldBeRequired — ĐẠT
- PartyMember FullName ShouldUseVietnameseCollation — ĐẠT
- BusinessEntities ShouldMapToUnderscoreTables (entityType: typeof(HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities.PartyMember), tableName: "party_member") — ĐẠT

## TestTodayVariableTests

- WithoutTheVariable NothingIsForcedAndNothingIsWarned — ĐẠT
- InProduction TheVariableIsIgnoredAndWarned — ĐẠT
- OutsideProduction TheVariableForcesToday — ĐẠT

## AwardPeriodCoverageBuilderTests

- QT6 · Chưa cài đợt nào: dải vẫn là một khoảng trống cả năm, cảnh báo gaps rỗng — ĐẠT
- QT6 · Đợt nằm lọt trong đợt khác: báo chồng lấn, không cắt thêm đoạn — ĐẠT
- QT6 · Bộ đợt mẫu cho đúng một cặp chồng lấn kèm khoảng ngày dùng chung — ĐẠT
- QT6 · Bộ đợt mẫu cho đúng hai khoảng trống kèm tên hai đợt kề — ĐẠT
- UC-36 · Dải độ phủ liền mạch 01/01–31/12, phần chồng lấn thuộc đợt đến trước — ĐẠT
- QT6 · Đợt vắt qua 31/12 để lại hai đoạn trong cùng một năm — ĐẠT
- QT6 · Đợt vắt năm một mình không tự chồng lấn, chỉ hở khoảng giữa năm — ĐẠT
- QT2 · Đợt 29/02 ở năm không nhuận lùi về 28/02 — ĐẠT

## DateTimeProviderTests

- Today ShouldBeTheVietnamCalendarDay — ĐẠT
- Today ShouldIgnoreMalformedTestVariable (value: "2026-10-15T00:00:00") — ĐẠT
- Today ShouldIgnoreMalformedTestVariable (value: "15/10/2026") — ĐẠT
- TestTodayVariable ShouldKeepTheAgreedName — ĐẠT
- Today ShouldIgnoreMalformedTestVariable (value: "khong-phai-ngay") — ĐẠT
- Today ShouldIgnoreMalformedTestVariable (value: "2026-13-40") — ĐẠT
- Now ShouldUseVietnamOffset — ĐẠT
- Today ShouldFollowTestVariable OutsideProduction — ĐẠT
- Today ShouldIgnoreMalformedTestVariable (value: "   ") — ĐẠT
- Today ShouldIgnoreTestVariable InProduction — ĐẠT
- Now ShouldTrackUtcClock — ĐẠT

## PartyMemberImportRowValidatorTests

- Dòng đủ bốn ô hợp lệ thì không có lỗi và được chuẩn hóa — ĐẠT
- OQ-6: ngày nhận cả một chữ số lẫn hai chữ số(birth: "9/02/1975", admission: "01/10/1996") — ĐẠT
- OQ-10: ngày sinh bằng đúng ngày chính thức vẫn là lỗi — ĐẠT
- Ngày chính thức ở tương lai → FutureOfficialAdmissionDate — ĐẠT
- Ngày chính thức dạng yyyy-MM-dd → InvalidDateFormat — ĐẠT
- Ngày sinh và giới tính bỏ trống vẫn hợp lệ, trả null — ĐẠT
- OQ-1: ngày sinh 31/02/1974 không có thật → InvalidDateFormat — ĐẠT
- OQ-6: ngày nhận cả một chữ số lẫn hai chữ số(birth: "09/02/1975", admission: "01/10/1996") — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường(cell: "nam", expected: "Male") — ĐẠT
- OQ-2: một dòng nhiều lỗi trả đủ mọi lý do, đúng thứ tự bảng mã lỗi — ĐẠT
- Thiếu họ tên → MissingFullName — ĐẠT
- 29/02 năm không nhuận là ngày không có thật → InvalidDateFormat — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường(cell: "nữ", expected: "Female") — ĐẠT
- Ngày chính thức đúng bằng hôm nay là hợp lệ — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường(cell: "NAM", expected: "Male") — ĐẠT
- Ngày sinh sau ngày chính thức → BirthDateAfterAdmissionDate — ĐẠT
- OQ-6: ngày nhận cả một chữ số lẫn hai chữ số(birth: "9/2/1975", admission: "1/10/1996") — ĐẠT
- Giới tính lạ → InvalidGender — ĐẠT
- 29/02 năm nhuận là ngày có thật, không bị coi là sai định dạng — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường(cell: "Nữ", expected: "Female") — ĐẠT
- Thiếu ngày chính thức → MissingOfficialAdmissionDate, không kèm lỗi định dạng — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường(cell: "NỮ", expected: "Female") — ĐẠT

## SkeletonTests

- User ShouldSupportPasswordVerification — ĐẠT
- User ShouldBeAJwtUser — ĐẠT

## ExportEndpointTests

- A-705 · Tên đơn vị trống: dòng 1 là tên đợt, không có dòng trắng thừa — ĐẠT
- 8.1 → 8.3 · Thiếu token trả 401(path: "/api/Exports/Unassigned") — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Exports/Unassigned", year: 0) — ĐẠT
- 8.2 · Mọi đợt của năm nay đã qua: file mang đợt đầu năm sau — ĐẠT
- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9(periodCode: "P2", expectedKey: "Đợt 19/5 năm 2026") — ĐẠT
- 8.1 · Id đợt lạ trả Mes.AwardPeriod.NotFound — ĐẠT
- A-703 · Tên file chưa thuộc đợt nào là ChuaThuocDot_<năm>.xlsx — ĐẠT
- A-704, A-706, A-708, A-710 · Đợt 7/11 · 2026: tiêu đề, cột và 6 dòng dữ liệu — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Exports/Eligibility", year: 2201) — ĐẠT
- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9(periodCode: "P3", expectedKey: "Đợt 2/9 năm 2026") — ĐẠT
- 8.2 · Xuất Dashboard lấy đúng đợt sắp tới theo QT8 — ĐẠT
- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9(periodCode: "P1", expectedKey: "Đợt 3/2 năm 2026") — ĐẠT
- 8.1 → 8.3 · Thiếu token trả 401(path: "/api/Exports/Dashboard") — ĐẠT
- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9(periodCode: "P4", expectedKey: "Đợt 7/11 năm 2026") — ĐẠT
- 8.2 · Chưa cài đợt nào trả Mes.Dashboard.NotFound.UpcomingPeriod — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Exports/Eligibility", year: 1899) — ĐẠT
- A-709 · Không ai bị sót: file chưa thuộc đợt nào vẫn hợp lệ — ĐẠT
- 8.1, 8.3 · Bỏ trống year thì lấy năm hiện tại của máy chủ — ĐẠT
- A-709 · Danh sách rỗng vẫn trả file hợp lệ chỉ có phần tiêu đề — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Exports/Unassigned", year: 2201) — ĐẠT
- 8.3 · Chưa thuộc đợt nào 2026: 7 dòng, có cột Khoảng trống đúng nhãn — ĐẠT
- 8.1 → 8.3 · Thiếu token trả 401(path: "/api/Exports/Eligibility") — ĐẠT
- A-707 · E01 thiếu Ngày sinh và Giới tính: hai ô rỗng, không phải dấu gạch ngang — ĐẠT

## PartyMemberEndpointTests

- 3.1 · Lọc giới tính Nam / Nữ; bỏ tham số thì lấy tất cả(filter: "&filter.Gender=$eq:Male", expectedCount: 1, expectedName: "Cao Văn Phúc") — ĐẠT
- 3.6 · Xóa nhiều trả đúng id đã xóa, id lạ bị bỏ qua lặng lẽ — ĐẠT
- 3.1 · Lọc giới tính Nam / Nữ; bỏ tham số thì lấy tất cả(filter: "&filter.Gender=$eq:Female", expectedCount: 1, expectedName: "Bùi Thị Lan") — ĐẠT
- A-001 · Mọi endpoint đảng viên không kèm token trả 401(method: "POST", path: "/api/PartyMembers") — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400(fullName: null, dateOfBirth: null, gender: null, officialAdmissionDate: "1996-10-15", messagesType: "Required", property: "FullName") — ĐẠT
- T51 · Mốc không hợp lệ trả 400 kèm khóa Invalid.NextMilestone(value: "$eq:abc") — ĐẠT
- A-001 · Mọi endpoint đảng viên không kèm token trả 401(method: "POST", path: "/api/PartyMembers/DeleteMany") — ĐẠT
- 3.4 · Sửa id không tồn tại trả 400 Mes.PartyMember.NotFound — ĐẠT
- 3.1 · pageSize hoặc current không dương được kẹp về mặc định, vẫn trả 200(query: "?current=0") — ĐẠT
- T51 · Sắp xếp theo Mốc kế tiếp; nhóm không còn mốc đứng cuối khi tăng dần — ĐẠT
- 3.6 · Xóa một người qua mảng một phần tử trả đúng một id, bản ghi biến mất hẳn — ĐẠT
- 3.1 · Phân trang: mặc định 20 dòng, chọn được số dòng và số trang — ĐẠT
- T51 · Sắp xếp theo Tuổi đảng quy về Ngày chính thức theo chiều ngược lại — ĐẠT
- 3.6 · Xóa nhiều với danh sách rỗng trả 400 — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400(fullName: "Nguyễn Văn An", dateOfBirth: null, gender: null, officialAdmissionDate: "2026-09-20", messagesType: "Invalid", property: "OfficialAdmissionDate") — ĐẠT
- 3.1 · Lọc giới tính Nam / Nữ; bỏ tham số thì lấy tất cả(filter: "", expectedCount: 3, expectedName: null) — ĐẠT
- 3.3 · Thêm mới trả 200 kèm bản ghi vừa tạo và khóa Create.Successfully — ĐẠT
- 3.1 · Sắp xếp theo cột; cột lạ bị bỏ qua và quay về mặc định — ĐẠT
- T34 · DELETE /api/PartyMembers/{id} đã bị gỡ, không ai dựng lại được — ĐẠT
- 3.1 · Tìm theo họ tên chứa chuỗi, không phân biệt hoa thường — ĐẠT
- T51 · Mốc không hợp lệ trả 400 kèm khóa Invalid.NextMilestone(value: "$gt:30") — ĐẠT
- 3.6 · Xóa mảng chỉ có id lạ trả danh sách rỗng chứ không phải lỗi — ĐẠT
- T51 · Mốc không hợp lệ trả 400 kèm khóa Invalid.NextMilestone(value: "$eq:") — ĐẠT
- 3.1 · pageSize hoặc current không dương được kẹp về mặc định, vẫn trả 200(query: "?pageSize=0") — ĐẠT
- T51 · Lọc theo Mốc kế tiếp bám dãy mốc trong Cài đặt — ĐẠT
- 3.1 · Danh sách trả gender chuỗi và ba giá trị tuổi đảng tính theo hôm nay — ĐẠT
- T51 · Mốc không hợp lệ trả 400 kèm khóa Invalid.NextMilestone(value: "40") — ĐẠT
- 3.4 · Sửa ghi đè cả bốn trường, kể cả xóa bằng null — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400(fullName: "Nguyễn Văn An", dateOfBirth: "1996-10-16", gender: null, officialAdmissionDate: "1996-10-15", messagesType: "Invalid", property: "DateOfBirth") — ĐẠT
- 3.2 · Lấy một trả đúng bản ghi; id lạ trả 400 Mes.PartyMember.NotFound — ĐẠT
- 3.3 · Đúng ngày hôm nay là ngày chính thức hợp lệ — ĐẠT
- T51 · Mốc không hợp lệ trả 400 kèm khóa Invalid.NextMilestone(value: "$eq:33") — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400(fullName: "Nguyễn Văn An", dateOfBirth: null, gender: "Khac", officialAdmissionDate: "1996-10-15", messagesType: "Invalid", property: "Gender") — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400(fullName: "Nguyễn Văn An", dateOfBirth: null, gender: null, officialAdmissionDate: null, messagesType: "Required", property: "OfficialAdmissionDate") — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400(fullName: "   ", dateOfBirth: null, gender: null, officialAdmissionDate: "1996-10-15", messagesType: "Required", property: "FullName") — ĐẠT
- A-001 · Mọi endpoint đảng viên không kèm token trả 401(method: "GET", path: "/api/PartyMembers") — ĐẠT
- 3.3 · QT9 · Thêm hai người trùng hệt nhau vẫn thành hai bản ghi — ĐẠT
- T51 · Lọc theo Mốc kế tiếp chạy trong SQL nên tổng số dòng đúng — ĐẠT

## UpdatedAtStampTests

- Cài đặt seed sẵn mang dấu thời gian của IDateTimeProvider, không phải giờ máy — ĐẠT
- Sửa đảng viên ghi đè dấu thời gian cũ — ĐẠT
- Thêm mới đảng viên được đóng dấu dù không ai gán UpdatedAt — ĐẠT

## AwardPeriodEndpointTests

- 5.4 · Sửa sang tên đợt khác trả Repeated.Name; id lạ trả NotFound — ĐẠT
- 5.5 · Xóa hẳn đợt, trả khoảng trống mới và không đụng đảng viên (QT10) — ĐẠT
- 5.1 → 5.5 · Thiếu token trả 401(method: "POST", path: "/api/AwardPeriods") — ĐẠT
- 5.1 · Năm khác: trạng thái vẫn so với hôm nay, ngày gắn đúng năm được hỏi — ĐẠT
- 5.3 · Hai lỗi chặn lưu: thiếu tên, ngày không có thật(name: "Đợt 31/04", fromDay: 31, fromMonth: 4, toDay: 7, toMonth: 11, expectedKey: "Mes.AwardPeriod.Invalid.FromDate") — ĐẠT
- 5.3 · Trùng tên không phân biệt hoa thường trả Mes.AwardPeriod.Repeated.Name — ĐẠT
- 5.4 · Sửa đợt: giữ nguyên tên của chính nó, có hiệu lực ngay cho năm hiện tại — ĐẠT
- 5.1 → 5.5 · Thiếu token trả 401(method: "DELETE", path: "/api/AwardPeriods/9b7c0f9c-0000-0000-0000-00000000"···) — ĐẠT
- 5.2 · Lấy một đợt theo id và theo năm được hỏi; id lạ trả NotFound — ĐẠT
- 5.3 · Đợt vắt qua 31/12 được lưu, Đến ngày rơi vào năm sau — ĐẠT
- 5.1 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(query: "?year=2201") — ĐẠT
- 5.1 · Cảnh báo liệt kê đúng một cặp chồng lấn và một khoảng trống — ĐẠT
- 5.1 · Chưa cài đợt nào: cảnh báo gaps rỗng, dải vẫn phủ trọn 01/01–31/12 — ĐẠT
- 5.3 · Hai lỗi chặn lưu: thiếu tên, ngày không có thật(name: "Đợt đến 31/11", fromDay: 1, fromMonth: 10, toDay: 31, toMonth: 11, expectedKey: "Mes.AwardPeriod.Invalid.ToDate") — ĐẠT
- 5.1 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(query: "?year=1899") — ĐẠT
- 5.1 · Dải độ phủ liền mạch 01/01–31/12, phần chồng lấn thuộc đợt đến trước — ĐẠT
- 5.3 · Thêm đợt chồng lấn vẫn là 200 và trả kèm cảnh báo — ĐẠT
- 5.3 · Hai lỗi chặn lưu: thiếu tên, ngày không có thật(name: null, fromDay: 1, fromMonth: 10, toDay: 7, toMonth: 11, expectedKey: "Mes.AwardPeriod.Required.Name") — ĐẠT
- 5.1 · Bốn đợt mẫu: sắp theo fromDate, ba trạng thái và số người đủ điều kiện — ĐẠT
- 5.1 → 5.5 · Thiếu token trả 401(method: "GET", path: "/api/AwardPeriods") — ĐẠT

## EligibilityEndpointTests

- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Eligibility/Unassigned", year: 0) — ĐẠT
- 6.2 · Bỏ trống year thì lấy năm hiện tại của máy chủ — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Eligibility", year: 2201) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P2", year: 2027) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P3", year: 2025) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P1", year: 2027) — ĐẠT
- 6.3 · Đổi Bước 5 → 10 làm danh sách còn 6 người ngay (QT1, QT5) — ĐẠT
- 6.3 · Chưa cài đợt nào: mọi người tròn mốc đều rơi vào Trước đợt đầu tiên — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P2", year: 2028) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P3", year: 2026) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P1", year: 2028) — ĐẠT
- 6.2 · Id đợt lạ trả Mes.AwardPeriod.NotFound — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Eligibility", year: 1899) — ĐẠT
- 6.4 · Badge luôn bằng số dòng của màn Chưa thuộc đợt nào — ĐẠT
- 6.3 · Chưa thuộc đợt nào năm 2026: 7 người, đúng nhãn khoảng trống (QT7) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P1", year: 2026) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P4", year: 2026) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Eligibility/Unassigned", year: -5) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P3", year: 2027) — ĐẠT
- 6.3 · Các năm khác của bộ lõi khớp expected.json(year: 2027) — ĐẠT
- 6.2 · Nới Đến ngày Đợt 2/9 sang 30/09: đợt lên 5 người ngay, không lưu gì (QT5) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P4", year: 2027) — ĐẠT
- 6.3 · Các năm khác của bộ lõi khớp expected.json(year: 2025) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P4", year: 2028) — ĐẠT
- 6.2 → 6.4 · Thiếu token trả 401(path: "/api/Eligibility/UnassignedCount") — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P4", year: 2025) — ĐẠT
- 6.3 · Các năm khác của bộ lõi khớp expected.json(year: 2028) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P3", year: 2028) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P1", year: 2025) — ĐẠT
- 6.2 → 6.4 · Thiếu token trả 401(path: "/api/Eligibility") — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P2", year: 2026) — ĐẠT
- 6.2 → 6.4 · Thiếu token trả 401(path: "/api/Eligibility/Unassigned") — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P2", year: 2025) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Eligibility/UnassignedCount", year: 2201) — ĐẠT

## SettingsEndpointTests

- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400(body: "{\"startYears\":30.5,\"endYears\":90,\"stepYears\""···) — ĐẠT
- 7.3 · A-506 · Khôi phục mặc định đưa về 30/90/5 và giữ nguyên Tên đơn vị — ĐẠT
- 7.2 · A-508 · Tên đơn vị bỏ trống hoặc toàn khoảng trắng đều thành chưa đặt(unitName: "   ") — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường(startYears: 0, endYears: 90, property: "StartYears") — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường(startYears: 30, endYears: -30, property: "EndYears") — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400(body: "{\"endYears\":90,\"stepYears\":5}") — ĐẠT
- 7.2 · A-508 · Tên đơn vị bỏ trống hoặc toàn khoảng trắng đều thành chưa đặt(unitName: "") — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400(body: "{\"startYears\":\"\",\"endYears\":90,\"stepYears\""···) — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường(startYears: -1, endYears: 90, property: "StartYears") — ĐẠT
- 7.4 · Xem trước dãy mốc không ghi gì xuống cơ sở dữ liệu — ĐẠT
- 7.2 · A-502 · Đổi Bước 5 → 10: danh sách đủ điều kiện và badge đổi ngay (QT5) — ĐẠT
- 7.2 · A-509 · Lưu hai lần liên tiếp vẫn chỉ có đúng một bản ghi cài đặt — ĐẠT
- 7.2 · A-508 · Tên đơn vị bỏ trống hoặc toàn khoảng trắng đều thành chưa đặt(unitName: null) — ĐẠT
- 7.1 → 7.4 · Chưa đăng nhập thì cả bốn endpoint đều trả 401 — ĐẠT
- 7.2 · A-504 · Bước 0 hoặc âm trả 400 Mes.AppSetting.Invalid.StepYears(stepYears: -5) — ĐẠT
- 7.4 · Bỏ trống tham số nào thì lấy giá trị đang lưu của tham số đó — ĐẠT
- 7.2 · A-503 · Bắt đầu > Kết thúc trả 400 Mes.AppSetting.Invalid.Range — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400(body: "{\"startYears\":\"ba mươi\",\"endYears\":90,\"step"···) — ĐẠT
- 7.2 · Tên đơn vị quá 200 ký tự trả 400 Mes.AppSetting.OverLength.UnitName — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400(body: "{\"startYears\":null,\"endYears\":90,\"stepYears\""···) — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường(startYears: 30, endYears: 0, property: "EndYears") — ĐẠT
- 7.1 · A-501 · Kho chưa có bản ghi cài đặt nào thì đọc ra mặc định 30/90/5 — ĐẠT
- 7.2 · A-509 · Kho trống: lưu lần đầu tạo đúng một bản ghi, không nhân bản — ĐẠT
- 7.2 · A-504 · Bước 0 hoặc âm trả 400 Mes.AppSetting.Invalid.StepYears(stepYears: 0) — ĐẠT
- 7.2 · A-507 · Lưu Tên đơn vị: đọc lại thấy ngay, Dashboard cũng thấy — ĐẠT
- 7.1 · Kho đã seed: đọc ra 30/90/5, 13 mốc và tên đơn vị đang lưu — ĐẠT
- 7.4 · Tham số xem trước sai trả cùng bộ khóa lỗi với 7.2 — ĐẠT

## ImportEndpointTests

- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa(fileName: "loi-rong.xlsx", expectedProperty: "Empty") — ĐẠT
- 4.2 · loi-moi-loai-mot-dong.xlsx: 8 dòng lỗi, mỗi loại một dòng, dòng nhiều lỗi trả đủ lý do — ĐẠT
- 4.3 · Lỗi cấp file cũng chặn ở bước nạp(fileName: "loi-khong-phai-xlsx.xlsx", expectedProperty: "Extension") — ĐẠT
- 4.3 · Lỗi cấp file cũng chặn ở bước nạp(fileName: "loi-sai-cot.xlsx", expectedProperty: "Columns") — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa(fileName: "loi-chi-co-tieu-de.xlsx", expectedProperty: "NoDataRows") — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa(fileName: "loi-sai-cot.xlsx", expectedProperty: "Columns") — ĐẠT
- 4.1 · File mẫu hệ thống sinh khớp fixture mau-dang-vien.xlsx của QC — ĐẠT
- 4.2 · File quá 10 MB bị chặn trước khi mở, trả Mes.Import.Invalid.FileSize — ĐẠT
- Ba endpoint đều yêu cầu token — ĐẠT
- 4.3 · Nạp loi-4-dong.xlsx: thêm 6 người, bỏ qua 4 dòng lỗi, không kiểm tra trùng — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa(fileName: "loi-khong-phai-xlsx.xlsx", expectedProperty: "Extension") — ĐẠT
- 4.2 · loi-4-dong.xlsx: 10 dòng, 6 hợp lệ, 4 lỗi ở dòng 8, 9, 10, 11 — ĐẠT
- 4.2 · bien-chuan-hoa.xlsx: cắt khoảng trắng (OQ-4), giới tính hoa thường (OQ-5), ngày một chữ số (OQ-6) — ĐẠT
- 4.2 · Bộ lõi 32 dòng hợp lệ, đọc được cả ngày dạng chuỗi lẫn ngày kiểu ngày(fileName: "core-hop-le-ngay-kieu-date.xlsx") — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa(fileName: "loi-dinh-dang-csv.csv", expectedProperty: "Extension") — ĐẠT
- 4.1 · File mẫu trả file nhị phân đúng tên, 4 cột đúng thứ tự, 2 dòng ví dụ dd/MM/yyyy — ĐẠT
- 4.2 · Xem trước không ghi gì vào cơ sở dữ liệu — ĐẠT
- 4.3 · Nạp file toàn lỗi: không thêm ai, không ném lỗi — ĐẠT
- 4.2 · Bộ lõi 32 dòng hợp lệ, đọc được cả ngày dạng chuỗi lẫn ngày kiểu ngày(fileName: "core-hop-le.xlsx") — ĐẠT

## PartyMemberFilterValueTests

- QC-T27-05 · Giá trị lọc hợp lệ vẫn lọc đúng như trước — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter(filter: "filter.Gender=$eq:Khac") — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter(filter: "filter.Gender=$in:Male,Khac") — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter(filter: "filter.Gender=$eq:") — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter(filter: "filter.Gender=Male") — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter(filter: "filter.Gender=$eq:1") — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter(filter: "filter.OfficialAdmissionDate=$gte:hom-qua") — ĐẠT

## QueryParameterGuardTests

- QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh(url: "/api/AwardPeriods?year=khong-phai-so") — ĐẠT
- QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh(url: "/api/AwardPeriods?year=2147483648") — ĐẠT
- QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh(url: "/api/Eligibility?awardPeriodId=khong-phai-guid") — ĐẠT
- QC-T27-02 · Số trang nhỏ hơn 1 được coi như trang 1 — ĐẠT
- QC-T27-06 · Xem trước và lưu dùng đúng một bộ luật — ĐẠT
- QC-T27-03 · pageSize nhỏ hơn 1 lấy mặc định 20 — ĐẠT
- QC-T27-02 · Số trang rất lớn trả 200 với danh sách rỗng, không 500 — ĐẠT
- QC-T27-04 · Khóa của FluentValidation vẫn đi thẳng ra ngoài — ĐẠT
- QC-T27-06 · Xem trước dãy mốc từ chối khoảng vượt trần, đúng khóa của PUT — ĐẠT
- QC-T27-06 · Khoảng hợp lệ vẫn xem trước được bình thường — ĐẠT
- QC-T27-03 · pageSize vượt trần bị kẹp về 200, không báo lỗi — ĐẠT
- QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh(url: "/api/PartyMembers?current=khong-phai-so") — ĐẠT

## AwardPeriodFilterValueTests

- QC-T27-05 · Giá trị lọc lạ trên danh sách đợt trả 400, không trả cả kho(filter: "filter.FromMonth=$eq:") — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trên danh sách đợt trả 400, không trả cả kho(filter: "filter.FromMonth=$eq:khong-phai-so") — ĐẠT
- 5.1 · Giá trị lọc hợp lệ lọc đúng; cảnh báo và độ phủ vẫn tính trên cả năm — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trên danh sách đợt trả 400, không trả cả kho(filter: "filter.ToDay=$gte:hom-qua") — ĐẠT
- QC-T27-05 · Kho rỗng cũng phải từ chối giá trị lọc sai kiểu — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trên danh sách đợt trả 400, không trả cả kho(filter: "filter.SpansNextYear=$eq:co") — ĐẠT
- 5.1 · Tên trường không tồn tại vẫn được bỏ qua, không phải lỗi — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trên danh sách đợt trả 400, không trả cả kho(filter: "filter.Id=$eq:khong-phai-guid") — ĐẠT

## AuthEndpointTests

- Thiếu tài khoản hoặc mật khẩu trả 400 kèm khóa Required(username: "admin", password: "", expectedProperty: "Password") — ĐẠT
- Đăng xuất có token trả 200 và khóa Mes.User.Logout.Successfully — ĐẠT
- A-001 · Gọi endpoint nghiệp vụ không kèm token trả 401 và không lộ dữ liệu(method: "GET", path: "/api/Auth/Me") — ĐẠT
- Token hợp lệ nhưng chủ thể không còn tồn tại trả 401 — ĐẠT
- A-006 · Token vừa cấp dùng được ngay cho endpoint khác — ĐẠT
- Hạn token đúng 8 giờ theo hợp đồng mục 1.2 — ĐẠT
- A-002 · JWT sai chữ ký trả 401 — ĐẠT
- A-006 · Đăng nhập đúng trả 200 kèm token đúng hợp đồng — ĐẠT
- A-003 · JWT đã hết hạn trả 401 — ĐẠT
- A-001 · Gọi endpoint nghiệp vụ không kèm token trả 401 và không lộ dữ liệu(method: "POST", path: "/api/Auth/Logout") — ĐẠT
- Thiếu tài khoản hoặc mật khẩu trả 400 kèm khóa Required(username: "", password: "Kiem@Thu123", expectedProperty: "Username") — ĐẠT
- A-004 · JWT thiếu tiền tố Bearer trả 401 — ĐẠT
- A-005 · Sai mật khẩu và không có tài khoản trả cùng một thông báo 401 — ĐẠT

## ImportTransactionTests

- 4.3 · Cùng đường đi đó nhưng không hỏng: bulk-1200.xlsx thêm đủ 1200 người — ĐẠT
- 4.3 · Hỏng đúng lúc chốt giao dịch: 32 dòng của core-hop-le.xlsx bị thu hồi hết — ĐẠT
- 4.3 · Hỏng ở lô thứ hai của bulk-1200.xlsx: 200 dòng đầu đã ghi vẫn bị thu hồi hết — ĐẠT

## PartyMemberSqlTranslationTests

- T51 · Lọc Mốc kế tiếp sinh ra hai bất đẳng thức trong WHERE — ĐẠT
- T51 · Sắp xếp theo cột tính ra sinh ra ORDER BY trên cột ngày(sortQuery: "NextMilestone desc", isDescending: False) — ĐẠT
- T51 · Sắp xếp theo cột tính ra sinh ra ORDER BY trên cột ngày(sortQuery: "PartyAge asc", isDescending: True) — ĐẠT
- T51 · Lọc None chỉ chặn cận trên — ĐẠT
- T51 · Sắp xếp theo cột tính ra sinh ra ORDER BY trên cột ngày(sortQuery: "NextMilestone asc", isDescending: True) — ĐẠT
- T51 · Sắp xếp theo cột tính ra sinh ra ORDER BY trên cột ngày(sortQuery: "partyAgeYears asc", isDescending: True) — ĐẠT
- T51 · Sắp xếp theo cột tính ra sinh ra ORDER BY trên cột ngày(sortQuery: "PartyAge desc", isDescending: False) — ĐẠT

## DashboardEndpointTests

- 6.1 · T1 = 15/10/2026: Đợt 7/11 đang diễn ra, daysRemaining = null — ĐẠT
- 6.1 · Cảnh báo của Dashboard trùng khớp với cảnh báo màn Đợt — ĐẠT
- 6.1 · T0 = 19/09/2026: đợt sắp tới là Đợt 7/11 · 2026, còn 12 ngày, 6 người — ĐẠT
- 6.1 · Chưa cài đợt nào: upcomingPeriod = null, bảng rỗng, cảnh báo noPeriods — ĐẠT
- 6.1 · Thiếu token trả 401 — ĐẠT
- 6.1 · T2 = 01/12/2026: mọi đợt 2026 đã qua nên đợt sắp tới là Đợt 3/2 · 2027 (QT8) — ĐẠT
- 6.1 · Kho trống hoàn toàn: đủ hai cảnh báo cho khối hướng dẫn ba bước (UC-13) — ĐẠT
- 6.1 · Chưa có đảng viên nào: cảnh báo noMembers, đợt sắp tới vẫn có, 0 người — ĐẠT

## AnonymousEndpointTests

- A-007 · Chỉ POST /api/Auth/Login được [AllowAnonymous] — ĐẠT
- Mọi controller đều kế thừa BaseController nên mặc định cần đăng nhập — ĐẠT

## EligibilityPerformanceTests

- 6.1 → 6.4 · 10.000 đảng viên: mỗi endpoint tính toán dưới 1 giây — ĐẠT

## A2AwardPeriodTests

- A220 Bo lon khong gay nhieu cho bo loi — ĐẠT
- A202 A203 Trang thai dot tai T0 va T1 — ĐẠT
- A217 Du dieu kien Dot 3 2 nam 2026 — ĐẠT
- A205 A206 A207 Ba rang buoc chan luu — ĐẠT
- A213 Xoa dot khong lam mat dang vien — ĐẠT
- A208 Dot bat dau 29 02 hop le va thu ve 28 02 o nam khong nhuan — ĐẠT
- A211 Bo dot phu kin khong con canh bao — ĐẠT
- A209 Hai dot chong lan van luu duoc va co canh bao dung cap — ĐẠT
- A218 Nam bat thuong duoc xu ly tat dinh — ĐẠT
- A215 A216 Du dieu kien o nam khac — ĐẠT
- A204 So nguoi du dieu kien nam nay tren tung dong — ĐẠT
- A212 Noi den ngay lam danh sach doi ngay — ĐẠT
- A214 Du dieu kien Dot 7 11 nam 2026 — ĐẠT
- A201 Bon dot chinh sap theo tu ngay — ĐẠT
- A219 Dot khong ton tai tra loi nghiep vu — ĐẠT
- A210 Bon dot chinh canh bao dung nam khoang trong — ĐẠT

## A6ImportTests

- A607 A611 Loi cap file bi chan ngay (fileName: "loi-khong-phai-xlsx.xlsx", expectedKey: "Mes.Import.Invalid.Extension") — ĐẠT
- A609 File chi co tieu de bao khac file rong — ĐẠT
- A607 A611 Loi cap file bi chan ngay (fileName: "loi-dinh-dang-csv.csv", expectedKey: "Mes.Import.Invalid.Extension") — ĐẠT
- A602 Nap sau xem truoc tang dung 32 — ĐẠT
- A615 Gui hai file duoc xu ly tat dinh — ĐẠT
- A604 Nap file bon dong loi chi them sau nguoi — ĐẠT
- A619 File mau dung cot va nap lai duoc — ĐẠT
- A606 Nap hai lan cung mot file tang gap doi — ĐẠT
- A621 Mot dong nhieu loi tra du moi ly do — ĐẠT
- A618 Nap file 1200 dong — ĐẠT
- A607 A611 Loi cap file bi chan ngay (fileName: "loi-rong.xlsx", expectedKey: "Mes.Import.Invalid.Empty") — ĐẠT
- A607 A611 Loi cap file bi chan ngay (fileName: "loi-chi-co-tieu-de.xlsx", expectedKey: "Mes.Import.Invalid.NoDataRows") — ĐẠT
- A601 Xem truoc file loi 32 dong hop le va chua ghi gi — ĐẠT
- A613 File hop le 9 9MB van duoc chap nhan — ĐẠT
- A620 O ngay kieu ngay cua Excel van doc duoc — ĐẠT
- A603 Xem truoc file bon dong loi dung so dong va dung ly do — ĐẠT
- A616 Cat ket noi giua luc nap khong de lai du lieu nua voi — ĐẠT
- A617 Loi ky thuat giua chung cuon lai toan bo — ĐẠT
- A607 A611 Loi cap file bi chan ngay (fileName: "loi-sai-cot.xlsx", expectedKey: "Mes.Import.Invalid.Columns") — ĐẠT
- A605 Chi xem truoc roi bo thi tong khong doi — ĐẠT
- A612 File qua 10MB bi chan truoc khi doc noi dung — ĐẠT
- A614 Khong gui file tra 400 — ĐẠT

## A3DashboardTests

- A303 Dashboard tai T2 nhay sang nam sau — ĐẠT
- A301 Dashboard tai T0 — ĐẠT
- A306 Kho trong hoan toan — ĐẠT
- A302 Dashboard tai T1 dang dien ra — ĐẠT
- A305 Khong co dang vien nao — ĐẠT
- A308 Canh bao tren Dashboard khop voi man Dot — ĐẠT
- A307 Badge chua thuoc dot nao bang 7 — ĐẠT
- A304 Khong co dot nao bao chua cai dot — ĐẠT

## A7ExportTests

- A704 Dong tieu de du ba phan — ĐẠT
- A701 A702 Ten file dung quy uoc — ĐẠT
- A710 Ba endpoint xuat deu mo duoc bang thu vien doc Excel — ĐẠT
- A706 A707 A708 Noi dung file khop bang dang xem — ĐẠT
- A709 Xuat danh sach rong van ra file hop le — ĐẠT
- A705 Ten don vi de trong thi bo dong do — ĐẠT
- A703 Ten file chua thuoc dot nao — ĐẠT
- A712 File chua thuoc dot nao co cot khoang trong — ĐẠT
- A711 Xuat tu Dashboard theo dot sap toi — ĐẠT

## A0AuthorizationTests

- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/PartyMembers") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/PartyMembers/DeleteMany") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/PartyMembers/Import/Preview") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "PUT", url: "/api/PartyMembers/00000000-0000-0000-0000-00000000"···) — ĐẠT
- A005 Dang nhap sai tra 401 va thong diep chung — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Auth/Me") — ĐẠT
- A004 Thieu tien to Bearer tra 401 — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/PartyMembers/Import/Commit") — ĐẠT
- A003 Jwt het han tra 401 — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/AwardPeriods/00000000-0000-0000-0000-00000000"···) — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "PUT", url: "/api/Settings") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Exports/Dashboard") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/Auth/Logout") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Exports/Unassigned") — ĐẠT
- A002 Jwt sai chu ky tra 401 — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/Settings/RestoreDefaults") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "PUT", url: "/api/AwardPeriods/00000000-0000-0000-0000-00000000"···) — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/PartyMembers") — ĐẠT
- A006 Dang nhap dung tra token dung duoc ngay — ĐẠT
- A007 Chi Login duoc phep AllowAnonymous — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Eligibility/UnassignedCount") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Settings") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Eligibility?awardPeriodId=00000000-0000-0000-"···) — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Dashboard") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Exports/Eligibility?awardPeriodId=00000000-00"···) — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/AwardPeriods") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/AwardPeriods") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/PartyMembers/00000000-0000-0000-0000-00000000"···) — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Eligibility/Unassigned") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/PartyMembers/Import/Template") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Settings/Milestones") — ĐẠT
- A001 Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "DELETE", url: "/api/AwardPeriods/00000000-0000-0000-0000-00000000"···) — ĐẠT

## A8DefectTests

- QcT2705 Gia tri loc la khong duoc bo qua lang le — ĐẠT
- QcT2706 Xem truoc day moc phai co tran — ĐẠT
- QcT2707 Moi khoa Backend tra ra deu phai co trong hop dong — ĐẠT
- QcT2703 Co trang phai bi chan hoac kep ve muc tran — ĐẠT
- QcT2702 So trang lon khong duoc lam may chu loi 500 — ĐẠT
- QcT2704 Loi ep kieu tham so phai tra khoa thong diep — ĐẠT

## A5SettingsTests

- A507 A508 Ten don vi luu duoc va de trong duoc — ĐẠT
- A506 Khoi phuc mac dinh khong dung ten don vi — ĐẠT
- A503 A504 A505 Cai dat sai bi tu choi — ĐẠT
- A511 Dang xuat tra 200 va khong huy token phia may chu — ĐẠT
- A502 Doi buoc lam moi danh sach doi theo ngay — ĐẠT
- A509 Luu hai lan van chi mot ban ghi cai dat — ĐẠT
- A510 Xem truoc day moc khong ghi gi vao co so du lieu — ĐẠT
- A501 Kho trong van tra cai dat mac dinh — ĐẠT

## A1PartyMemberTests

- A113 Loc gioi tinh dung so luong — ĐẠT
- A106 Co trang bat thuong khong lam treo may chu — ĐẠT
- A123 Xoa id khong ton tai va duong xoa mot da bi go — ĐẠT
- A124 Xoa danh sach co id trung khong dem trung — ĐẠT
- A120 Sua ngay chinh thuc lam moi thu doi theo ngay — ĐẠT
- A102 Trang dau 20 dong va 62 trang — ĐẠT
- A125 Tong so va tuoi dang tinh den hom nay — ĐẠT
- A122 Xoa nhieu nguoi giam dung so luong — ĐẠT
- A108 A109 Tim kiem khong phan biet hoa thuong — ĐẠT
- A117 Them tay thieu ho ten bi tu choi — ĐẠT
- A121 Xoa mot nguoi giam dung mot — ĐẠT
- A103 Trang cuoi con 12 dong — ĐẠT
- A116 Them tay hop le tang dung mot nguoi — ĐẠT
- A118 A119 Bien ngay chinh thuc bang hom nay — ĐẠT
- A115 Sap theo Ho ten dung bang chu cai tieng Viet — ĐẠT
- A101 Tong so dang vien dung 1232 — ĐẠT
- A105 Co trang 50 va 100 dung so trang — ĐẠT
- A114 Sap xep hai chieu dung tren moi cot — ĐẠT
- A107 Ghep moi trang khong trung khong thieu — ĐẠT
- A126 Lay mot dang vien theo id — ĐẠT
- A110 A111 Tim dung mot nguoi va tim khong thay — ĐẠT
- A104 Trang vuot qua trang cuoi tra rong khong loi — ĐẠT
- A112 Ky tu dac biet khong gay loi va khong bi hieu la dai dien — ĐẠT

## A4UnassignedTests

- A402 Doi buoc sang 10 con sau nguoi — ĐẠT
- A403 Nam 2025 va 2027 khop ket qua mong doi — ĐẠT
- A405 Sap theo Moc roi Ho ten — ĐẠT
- A401b Chua cai dot nao thi nhan la Truoc dot dau tien — ĐẠT
- A404 Bo dot phu kin thi khong ai bi sot — ĐẠT
- A401 Nam 2026 dung bay nguoi va dung nhan khoang trong — ĐẠT
- A406 Badge luon khop so dong cua man hinh — ĐẠT

## A9TechnicalTests

- A903 Bien ep ngay khong co hieu luc o Production — ĐẠT
- A906 Moi truong ngay nghiep vu la ngay thuan — ĐẠT
- A905 Moi thong bao loi deu tra khoa dich duoc va khong lo noi bo — ĐẠT
- A901 Khong noi nao trong src doc dong ho may — ĐẠT
- A902 Ket qua khong phu thuoc mui gio cua tien trinh — ĐẠT
- A903b Ngoai Production bien ep ngay phai co hieu luc — ĐẠT
- A904 Mot van dang vien van tinh duoi mot giay — ĐẠT

## A10ContractKeyTests

- A-906 · Bảng khóa của QC trùng khít mục 1.5 hợp đồng API — ĐẠT

## A2bSpanningYearTests

- A224 Xuat excel dot vat nam dung ten file va dong tieu de — ĐẠT
- A222 Nguoi tron moc 20 01 khong bi xep vao chua thuoc dot nao — ĐẠT
- A221 Danh sach dot vat nam gan dung nam va cho hai doan do phu — ĐẠT
- A223 Dashboard doc dung lan dien ra neo o nam truoc — ĐẠT
