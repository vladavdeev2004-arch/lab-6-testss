# Реестр 23 тестовых случаев

6 RejectEscape + 3 отдельных проверки PDF/EPUB + 3 EntryLimit + 2 ExpandedSizeLimit + 6 UploadContract + 3 отдельных проверки API/SQL/rate limit = 23. Это 10 методов: 6 Fact и 4 Theory.

Полный исполняемый листинг: Library.Tests/UploadTests.cs.
Исходный журнал (не повторный запуск): artifacts/xunit-results.json.

1. `UploadTests.RateLimitReturns429` — Passed в исходном журнале.
2. `UploadTests.EpubAccepted` — Passed в исходном журнале.
3. `UploadTests.EntryLimit(count: 51, accepted: False)` — Passed в исходном журнале.
4. `UploadTests.EntryLimit(count: 0, accepted: False)` — Passed в исходном журнале.
5. `UploadTests.EntryLimit(count: 50, accepted: True)` — Passed в исходном журнале.
6. `UploadTests.PdfAccepted` — Passed в исходном журнале.
7. `UploadTests.OldClientStillReadsV1` — Passed в исходном журнале.
8. `UploadTests.QueryIsParameterized` — Passed в исходном журнале.
9. `UploadTests.RejectEscape(name: "C:\\escape.pdf")` — Passed в исходном журнале.
10. `UploadTests.RejectEscape(name: "../escape.pdf")` — Passed в исходном журнале.
11. `UploadTests.RejectEscape(name: "a\\escape.pdf")` — Passed в исходном журнале.
12. `UploadTests.RejectEscape(name: "/tmp/escape.pdf")` — Passed в исходном журнале.
13. `UploadTests.RejectEscape(name: "a/../escape.pdf")` — Passed в исходном журнале.
14. `UploadTests.RejectEscape(name: "a/../../escape.pdf")` — Passed в исходном журнале.
15. `UploadTests.UploadContract(name: "../escape.pdf", role: "librarian", title: "Book", status: 400)` — Passed в исходном журнале.
16. `UploadTests.UploadContract(name: "book.exe", role: "librarian", title: "Book", status: 400)` — Passed в исходном журнале.
17. `UploadTests.UploadContract(name: "book.pdf", role: "librarian", title: "", status: 400)` — Passed в исходном журнале.
18. `UploadTests.UploadContract(name: "book.pdf", role: "reader", title: "Book", status: 403)` — Passed в исходном журнале.
19. `UploadTests.UploadContract(name: "book.pdf", role: "librarian", title: "Book", status: 202)` — Passed в исходном журнале.
20. `UploadTests.UploadContract(name: "book.pdf", role: "", title: "Book", status: 401)` — Passed в исходном журнале.
21. `UploadTests.ExpandedSizeLimit(size: 10485760, accepted: True)` — Passed в исходном журнале.
22. `UploadTests.ExpandedSizeLimit(size: 10485761, accepted: False)` — Passed в исходном журнале.
23. `UploadTests.ExecutableRejected` — Passed в исходном журнале.