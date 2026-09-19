import { PageHeading } from '../../components/PageHeading';
import { PagePlaceholder } from '../../components/PagePlaceholder';

/** Dashboard — khung màn hình, nội dung dựng ở task riêng. */
export default function DashboardPage() {
  return (
    <>
      <PageHeading title="Dashboard" />
      <PagePlaceholder screen="Dashboard" />
    </>
  );
}
