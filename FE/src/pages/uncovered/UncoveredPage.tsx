import { PageHeading } from '../../components/PageHeading';
import { PagePlaceholder } from '../../components/PagePlaceholder';

/** Chưa thuộc đợt nào — khung màn hình, nội dung dựng ở task riêng. */
export default function UncoveredPage() {
  return (
    <>
      <PageHeading title="Chưa thuộc đợt nào" />
      <PagePlaceholder screen="Chưa thuộc đợt nào" />
    </>
  );
}
