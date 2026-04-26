import { ReactNode } from "react";

interface Props {
  children: ReactNode;
}

function CommonTemplate({ children }: Props) {
  return (
    <>
      <header className="container">header</header>
      {children}
      <footer className="container">footer</footer>
    </>
  );
}

export default CommonTemplate;
