// The only event names a page may record, each with its parameters. A name that is not
// here, or a parameter that does not belong to it, does not compile.
//
// A module team may add a name here in its own pull request; the platform team reviews it.
// A name is <object>_<past-tense action>, lowercase with underscores (tour_published).
// A value is text or a number, and never a user ID, free text the user typed (a title, a message, a search), a name or an email.
export interface AnalyticsEvents {
  tour_published: { tour_id: string };
  gift_card_purchased: { amount: number };
  quiz_published: { quiz_id: string };
  challenge_published: { challenge_id: string };
  blog_published: { blog_id: string };
  cart_purchased: { tour_count: number; total_in_ec: number; discount_in_ec: number };
}
